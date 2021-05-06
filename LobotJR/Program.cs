using Adventures;
using Classes;
using Companions;
using Equipment;
using Fishing;
using GroupFinder;
using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Modules.Fishing;
using LobotJR.Shared.Authentication;
using LobotJR.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Wolfcoins;

namespace TwitchBot
{

    public class Better
    {
        public int betAmount;
        public int vote;

        public Better()
        {
            betAmount = -1;
            vote = -1;
        }
    }

    class Program
    {

        static void Whisper(string user, string message, IrcClient whisperClient)
        {
            string toSend = ".w " + user + " " + message;
            whisperClient.sendChatMessage(toSend);
        }

        static void Whisper(Party party, string message, IrcClient whisperClient)
        {
            for (int i = 0; i < party.NumMembers(); i++)
            {
                string toSend = ".w " + party.members.ElementAt(i).name + " " + message;
                whisperClient.sendChatMessage(toSend);
            }
        }

        static void UpdateDungeons(string dungeonListPath, ref Dictionary<int, string> dungeonList)
        {
            IEnumerable<string> fileText;
            if (File.Exists(dungeonListPath))
            {
                fileText = File.ReadLines(dungeonListPath, UTF8Encoding.Default);
            }
            else
            {
                fileText = new List<string>();
                Console.WriteLine($"Failed to load dungeon list file, {dungeonListPath} not found.");
            }

            dungeonList = new Dictionary<int, string>();
            int dungeonIter = 1;
            foreach (var line in fileText)
            {
                string[] temp = line.Split(',');
                int id = -1;
                int.TryParse(temp[0], out id);
                if (id != -1)
                    dungeonList.Add(id, temp[1]);
                else
                    Console.WriteLine("Invalid dungeon read on line " + dungeonIter);
                dungeonIter++;
            }
        }

        static void UpdateItems(string itemListPath, ref Dictionary<int, string> itemList, ref Dictionary<int, Item> itemDatabase)
        {
            IEnumerable<string> fileText;
            if (File.Exists(itemListPath))
            {
                fileText = File.ReadLines(itemListPath, UTF8Encoding.Default);
            }
            else
            {
                fileText = new List<string>();
                Console.WriteLine($"Failed to load item list file, {itemListPath} not found.");
            }
            itemDatabase = new Dictionary<int, Item>();
            itemList = new Dictionary<int, string>();
            int itemIter = 1;
            // ALERT: Was there a reason you were loading this from the file twice?
            // fileText = System.IO.File.ReadLines(itemListPath, UTF8Encoding.Default);
            foreach (var line in fileText)
            {
                string[] temp = line.Split(',');
                int id = -1;
                int.TryParse(temp[0], out id);
                if (id != -1)
                    itemList.Add(id, "content/items/" + temp[1]);
                else
                    Console.WriteLine("Invalid item read on line " + itemIter);
                itemIter++;
            }

            itemIter = 0;
            foreach (var item in itemList)
            {
                Item myItem = new Item();
                int parsedInt = -1;
                int line = 0;
                string[] temp = { "" };
                fileText = System.IO.File.ReadLines(itemList.ElementAt(itemIter).Value, UTF8Encoding.Default);
                // item ID
                myItem.itemID = itemList.ElementAt(itemIter).Key;
                // item name
                temp = fileText.ElementAt(line).Split('=');
                myItem.itemName = temp[1];
                line++;
                // item type (1=weapon, 2=armor, 3=other)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.itemType = parsedInt;
                line++;
                // Class designation (1=Warrior,2=Mage,3=Rogue,4=Ranger,5=Cleric)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.forClass = parsedInt;
                line++;
                // Item rarity (1=Uncommon,2=Rare,3=Epic,4=Artifact)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.itemRarity = parsedInt;
                line++;
                // success boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.successChance = parsedInt;
                line++;
                // item find (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.itemFind = parsedInt;
                line++;
                // coin boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.coinBonus = parsedInt;
                line++;
                // xp boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.xpBonus = parsedInt;
                line++;
                // prevent death boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myItem.preventDeathBonus = parsedInt;
                line++;
                // item description
                temp = fileText.ElementAt(line).Split('=');
                myItem.description = temp[1];

                itemDatabase.Add(itemIter, myItem);

                itemIter++;
            }

        }

        static void UpdateCoins(string user, int amount, Currency currency)
        {

        }

        static void UpdatePets(string petListPath, ref Dictionary<int, string> petList, ref Dictionary<int, Pet> petDatabase)
        {
            IEnumerable<string> fileText;
            if (File.Exists(petListPath))
            {
                fileText = File.ReadLines(petListPath, UTF8Encoding.Default);
            }
            else
            {
                fileText = new List<string>();
                Console.WriteLine($"Failed to load item list file, {petListPath} not found.");
            }
            petDatabase = new Dictionary<int, Pet>();
            petList = new Dictionary<int, string>();
            int petIter = 1;
            foreach (var line in fileText)
            {
                string[] temp = line.Split(',');
                int id = -1;
                int.TryParse(temp[0], out id);
                if (id != -1)
                    petList.Add(id, "content/pets/" + temp[1]);
                else
                    Console.WriteLine("Invalid pet read on line " + petIter);
                petIter++;
            }

            petIter = 0;
            foreach (var pet in petList)
            {
                Pet mypet = new Pet();
                int parsedInt = -1;
                int line = 0;
                string[] temp = { "" };
                fileText = System.IO.File.ReadLines(petList.ElementAt(petIter).Value, UTF8Encoding.Default);
                // pet ID
                mypet.ID = petList.ElementAt(petIter).Key;
                // pet name
                temp = fileText.ElementAt(line).Split('=');
                mypet.name = temp[1];
                line++;
                // pet type (string ex: Fox, Cat, Dog)
                temp = fileText.ElementAt(line).Split('=');
                mypet.type = temp[1];
                line++;
                // pet size (string ex: tiny, small, large)
                temp = fileText.ElementAt(line).Split('=');
                mypet.size = temp[1];
                line++;
                // pet rarity (1=Common,2=Uncommon,3=Rare,4=Epic,5=Artifact)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.petRarity = parsedInt;
                line++;
                // pet description
                temp = fileText.ElementAt(line).Split('=');
                mypet.description = temp[1];
                line++;
                // pet emote
                temp = fileText.ElementAt(line).Split('=');
                mypet.emote = temp[1];
                line++;
                int numLines = fileText.Count();
                if (numLines <= line)
                {
                    petDatabase.Add(petIter, mypet);
                    petIter++;
                    continue;
                }
                // success boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.successChance = parsedInt;
                line++;
                // item find (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.itemFind = parsedInt;
                line++;
                // coin boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.coinBonus = parsedInt;
                line++;
                // xp boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.xpBonus = parsedInt;
                line++;
                // prevent death boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.preventDeathBonus = parsedInt;
                line++;


                petDatabase.Add(petIter, mypet);

                petIter++;
            }

        }


        static void UpdateFish(string fishListPath, ref Dictionary<int, string> fishList, ref List<Fish> fishDatabase)
        {
            IEnumerable<string> fileText;
            if (File.Exists(fishListPath))
            {
                fileText = File.ReadLines(fishListPath, UTF8Encoding.Default);
            }
            else
            {
                fileText = new List<string>();
                Console.WriteLine($"Failed to load item list file, {fishListPath} not found.");
#if DEBUG
                fishList = new Dictionary<int, string>();
                fishList.Add(0, "content/fishing/smallfish");
                fishList.Add(1, "content/fishing/mediumfish");
                fishList.Add(2, "content/fishing/largefish");
                fishDatabase = new List<Fish>(new Fish[]
                {
                    new Fish()
                    {
                        ID = 1,
                        name = "smallfish",
                        sizeCategory = 1,
                        lengthRange = new float[] {5, 10},
                        weightRange = new float[] {100, 200},
                        rarity = 1,
                        flavorText = "A small fish."
                    },
                    new Fish()
                    {
                        ID = 2,
                        name = "mediumfish",
                        sizeCategory = 2,
                        lengthRange = new float[] {10, 20},
                        weightRange = new float[] {200, 400},
                        rarity = 2,
                        flavorText = "A medium fish."
                    },
                    new Fish()
                    {
                        ID = 3,
                        name = "largefish",
                        sizeCategory = 3,
                        lengthRange = new float[] {20, 40},
                        weightRange = new float[] {400, 800},
                        rarity = 3,
                        flavorText = "A large fish."
                    }
                });
                return;
#endif
            }
            fishDatabase = new List<Fish>();
            fishList = new Dictionary<int, string>();
            int fishIter = 0;
            foreach (var line in fileText)
            {
                fishIter++;
                fishList.Add(fishIter, "content/fishing/" + line);
            }

            fishIter = 0;
            foreach (var fish in fishList)
            {
                Fish myfish = new Fish();
                int parsedInt = -1;
                float parsedFloat = -1;
                int line = 0;
                string[] temp = { "" };
                fileText = System.IO.File.ReadLines(fishList.ElementAt(fishIter).Value, UTF8Encoding.Default);
                // fish ID
                myfish.ID = fishList.ElementAt(fishIter).Key;
                // fish name
                temp = fileText.ElementAt(line).Split('=');
                myfish.name = temp[1];
                line++;
                // fish size category
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myfish.sizeCategory = parsedInt;
                line++;
                // fish minimum length
                temp = fileText.ElementAt(line).Split('=');
                float.TryParse(temp[1], out parsedFloat);
                myfish.lengthRange[0] = parsedFloat;
                line++;
                // fish maximum length
                temp = fileText.ElementAt(line).Split('=');
                float.TryParse(temp[1], out parsedFloat);
                myfish.lengthRange[1] = parsedFloat;
                line++;
                // fish minimum weight
                temp = fileText.ElementAt(line).Split('=');
                float.TryParse(temp[1], out parsedFloat);
                myfish.weightRange[0] = parsedFloat;
                line++;
                // fish maximum weight
                temp = fileText.ElementAt(line).Split('=');
                float.TryParse(temp[1], out parsedFloat);
                myfish.weightRange[1] = parsedFloat;
                line++;
                // rarity
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myfish.rarity = parsedInt;
                line++;
                // fish description
                temp = fileText.ElementAt(line).Split('=');
                myfish.flavorText = temp[1];

                fishDatabase.Add(myfish);

                fishIter++;
            }

            fishDatabase.Sort((x, y) => x.rarity.CompareTo(y.rarity));
        }

        static Fish WeightedRandomFish(ref List<Fish> myFishDatabase)
        {
            if (myFishDatabase.Count <= 1)
                return new Fish();

            const float totalChance = 100;
            int numRarities = myFishDatabase.Last().rarity;
            float dividingFactor = totalChance / numRarities;
            List<float> chances = new List<float>();
            float updatedChance = totalChance;
            // algorithm to generate rarity %'s for each rarity (i.e., if there are 3 rarities, common/uncommon/epic,
            // assign each one a decreasing chance of it being picked ex: 66%/17%/8.5% based on this algorithm
            while (numRarities > 0)
            {
                chances.Add(dividingFactor * 2);
                updatedChance -= (dividingFactor * 2);
                numRarities--;
                if (numRarities == 2)
                {
                    // if last 2 rarities, assign 3/4 of remaining chance to 2nd to last and 1/4 to the last then break
                    chances.Add((3 * dividingFactor) / 4);
                    chances.Add(dividingFactor / 4);
                    break;
                }
                dividingFactor = (updatedChance / numRarities);
            }

            Random rng = new Random();
            // get a decimal rng value between 0 and 1.0 and scale it up to be 1 - 100
            float roll = ((float)rng.NextDouble() * 100);

            // using the assigned % for each rarity, equally distribute that % to each fish of that rarity
            // first find out the rarity to choose from (result)
            int result = 0;
            foreach (var chance in chances)
            {
                float check = totalChance - chance;
                result++;
                if (roll < check)
                    break;
            }

            // create a list of fish that have that rarity
            List<Fish> fishToChooseFrom = new List<Fish>();

            foreach (var fish in myFishDatabase)
            {
                if (fish.rarity == result)
                    fishToChooseFrom.Add(new Fish(fish));
            }

            // now that we have a list of fish of that rarity, equally random roll one
            int choice = rng.Next(0, fishToChooseFrom.Count);

            return fishToChooseFrom.ElementAt(choice);
        }
        #region TwitchPlaysData

        const int INPUT_MOUSE = 0;
        const int INPUT_KEYBOARD = 1;
        const int INPUT_HARDWARE = 2;

        const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        const int KEYEVENTF_KEYUP = 0x0002;

        const ushort KEYEVENTF_KEYDOWN = 0x0000;
        const ushort KEYEVENTF_SCANCODE = 0x0008;
        const ushort KEYEVENTF_UNICODE = 0x0004;

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern byte VkKeyScan(char ch);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            bool twitchPlays = false;
            bool connected = true;
            bool broadcasting = false;

            bool fishingTournamentActive = false;
            DateTime tournamentStart = DateTime.MinValue;
            DateTime nextTournament = DateTime.MinValue;

            int tournamentDuration = 15; // how long the tourney lasts in minutes
            int tournamentInterval = 30; // how long between the start of each tournament in minutes
            int tournamentCastTimeMax = 30; // max time it takes for a fish to bite in seconds. maybe scale between 1/2 of max time to max? 1/4?

            // How often to award Wolfcoins in minutes
            const int DUNGEON_MAX = 3;
            const int PARTY_FORMING = 1;
            const int PARTY_FULL = 2;
            const int PARTY_STARTED = 3;
            const int PARTY_COMPLETE = 4;
            const int PARTY_READY = 2;

            const int SUCCEED = 0;
            const int FAIL = 1;

            const int LOW_DETAIL = 0;
            const int HIGH_DETAIL = 1;

            int subCounter = 0;
            int awardMultiplier = 1;
            int awardInterval = 30;
            int awardAmount = 1;
            int awardTotal = 0;
            int gloatCost = 25;
            int pryCost = 1;

            Dictionary<int, Item> itemDatabase = new Dictionary<int, Item>();
            Dictionary<int, Pet> petDatabase = new Dictionary<int, Pet>();
            List<Fish> fishDatabase = new List<Fish>();

            var subathonPath = "C:/Users/Lobos/Dropbox/Stream/subathon.txt";
            IEnumerable<string> subathonFile;
            if (File.Exists(subathonPath))
            {
                subathonFile = File.ReadLines(subathonPath, UTF8Encoding.Default);
            }
            else
            {
                subathonFile = new List<string>();
                Console.WriteLine($"Failed to load subathon file, {subathonPath} not found.");
            }

            Dictionary<int, string> dungeonList = new Dictionary<int, string>();
            string dungeonListPath = "content/dungeonlist.ini";

            Dictionary<int, string> itemList = new Dictionary<int, string>();
            string itemListPath = "content/itemlist.ini";

            Dictionary<int, string> petList = new Dictionary<int, string>();
            string petListPath = "content/petlist.ini";

            Dictionary<int, string> fishingList = new Dictionary<int, string>();
            string fishingListPath = "content/fishlist.ini";

            Dictionary<int, Party> parties = new Dictionary<int, Party>();
            int maxPartyID = 0;

            GroupFinderQueue groupFinder;

            const int baseDungeonCost = 25;
            //const int baseRaidCost = 150;
            const int baseRespecCost = 250;

            Dictionary<string, Better> betters = new Dictionary<string, Better>();
            //string betStatement = "";
            bool betActive = false;
            bool betsAllowed = false;

            var clientData = FileUtils.ReadClientData();
            var tokenData = FileUtils.ReadTokenData();
            IrcClient irc = new IrcClient("irc.chat.twitch.tv", 80, tokenData.ChatUser, tokenData.ChatToken.AccessToken);
            IrcClient group = new IrcClient("irc.chat.twitch.tv", 80, tokenData.ChatUser, tokenData.ChatToken.AccessToken);
            // 199.9.253.119
            connected = irc.connected;

            #region Command Manager Setup
            var context = new SqliteContext();
            context.Database.Initialize(false);
            var repoManager = new SqliteRepositoryManager(context);
            var commandManager = new CommandManager(repoManager);
            commandManager.Initialize(tokenData.BroadcastUser, tokenData.ChatUser);
            commandManager.LoadAllModules();
            #endregion

            if (connected)
            {
                UpdateTokens(tokenData, clientData);
                Console.WriteLine($"Logged in as {tokenData.ChatUser}");
                irc.sendIrcMessage("twitch.tv/membership");
            }
            if (group.connected)
            {
                group.sendIrcMessage("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
            }
            if (!twitchPlays)
            {
                #region NormalBot
                string channel = tokenData.BroadcastUser;
                irc.joinRoom(channel);
                group.joinRoom("jtv");
                DateTime awardLast = DateTime.Now;
                Currency wolfcoins = new Currency(clientData);
                wolfcoins.UpdateViewers(channel);
                wolfcoins.UpdateSubs(tokenData.BroadcastToken.AccessToken, clientData.ClientId);


                UpdateDungeons(dungeonListPath, ref dungeonList);

                UpdateItems(itemListPath, ref itemList, ref itemDatabase);
                UpdatePets(petListPath, ref petList, ref petDatabase);
                UpdateFish(fishingListPath, ref fishingList, ref fishDatabase);

                // reset fishing tournament points
                foreach (var fisher in wolfcoins.fishingList)
                {
                    fisher.Value.tournamentPoints = 0;
                }

                groupFinder = new GroupFinderQueue(dungeonList);

                foreach (var member in wolfcoins.classList)
                {
                    member.Value.ClearQueue();
                }
                wolfcoins.SaveClassData();
                wolfcoins.SaveFishingList();

                DateTime lastConnectAttempt = DateTime.Now;

                while (true)
                {
                    if (!irc.connected)
                    {
                        UpdateTokens(tokenData, clientData);
                        if ((DateTime.Now - lastConnectAttempt).TotalSeconds > 5)
                        {
                            irc = new IrcClient("irc.chat.twitch.tv", 80, tokenData.ChatUser, tokenData.ChatToken.AccessToken);
                            group = new IrcClient("irc.chat.twitch.tv", 80, tokenData.ChatUser, tokenData.ChatToken.AccessToken);

                            lastConnectAttempt = DateTime.Now;
                            if (!connected)
                            {
                                Console.WriteLine("Disconnected from server. Attempting to reconnect in 30 seconds...");
                            }
                            else
                            {
                                Console.WriteLine("Reconnected to server.");
                            }
                        }
                        continue;
                    }

                    // message[0] has username, message[1] has message
                    string[] message = irc.readMessage(wolfcoins, channel);
                    string[] whispers = group.readMessage(wolfcoins, channel);
                    string whisperSender;
                    string whisperMessage;

                    if (irc.messageQueue.Count > 0)
                    {
                        irc.processQueue();
                    }

                    if (group.messageQueue.Count > 0)
                    {
                        group.processQueue();
                    }

                    // check if time has passed for a new tournament to spin up
                    if (broadcasting && (nextTournament - DateTime.Now).Ticks < 0)
                    {
                        // cancel all current casts
                        foreach (var fisher in wolfcoins.fishingList)
                        {
                            if (fisher.Value.isFishing)
                            {
                                fisher.Value.isFishing = false;
                                fisher.Value.fishHooked = false;
                                // Whisper(fisher.Value.username, "A new fishing tournament has begun! Your cast has been reset.", group);
                            }
                        }

                        // time for a tournament!
                        fishingTournamentActive = true;
                        //Whisper("lobosjr", "A tournament has begun!", group);

                        // set next tournament time
                        nextTournament = DateTime.Now.AddMinutes(tournamentInterval);

                        // set tournament start time
                        tournamentStart = DateTime.Now;

                        irc.sendChatMessage("A fishing tournament has just begun! For the next " + tournamentDuration + " minutes, fish can be caught more quickly & will be eligible for leaderboard recognition! Head to https://tinyurl.com/PlayWolfpackRPG and type !cast to play!");
                        Console.WriteLine("A fishing tournament kicked off at" + DateTime.Now.ToString());
                    }
                    // Console.WriteLine("Next tournament in: " + ((nextTournament - DateTime.Now).Ticks));

                    // if there's a fishing tournament going on, check to see if it's ended; if so, broadcast the tournament's end and turn off tournament mode
                    if (fishingTournamentActive)
                    {
                        if (!broadcasting)
                        {
                            int numParticipants = 0;
                            int topScore = 0;
                            string winner = "";
                            fishingTournamentActive = false;

                            foreach (var fisher in wolfcoins.fishingList)
                            {
                                if (fisher.Value.tournamentPoints != 0)
                                {
                                    numParticipants++;
                                }
                                else
                                {
                                    continue;
                                }

                                if (fisher.Value.tournamentPoints > topScore)
                                {
                                    topScore = fisher.Value.tournamentPoints;
                                    winner = fisher.Value.username;
                                }
                                fisher.Value.tournamentPoints = 0;

                            }
                            if (numParticipants > 0)
                            {
                                irc.sendChatMessage("Stream has gone offline, so the fishing tournament was ended early. D: Winner at the time of conclusion: " + winner + " with a score of " + topScore + ".");
                                Console.WriteLine("Fishing tournament ended at: " + DateTime.Now.ToString() + ". Winner: " + winner + ". Top Score: " + topScore + ".");
                            }
                            else
                            {
                                irc.sendChatMessage("Stream has gone offline. Fishing tournament ended.");
                            }
                            continue;
                        }

                        int maxDuration = tournamentDuration * 60; // value is in minutes, so convert to seconds to compare against time elapsed
                        int currentDuration = (int)((DateTime.Now - tournamentStart).TotalSeconds);
                        if (currentDuration > maxDuration)
                        {
                            int numParticipants = 0;
                            int topScore = 0;
                            string winner = "";
                            fishingTournamentActive = false;
                            var entries = wolfcoins.fishingList.Where(x => x.Value.tournamentPoints > 0).Select(x => new TournamentEntry(x.Value.username, x.Value.tournamentPoints)).ToList();

                            foreach (var fisher in wolfcoins.fishingList)
                            {
                                if (fisher.Value.tournamentPoints != 0)
                                {
                                    numParticipants++;
                                }
                                else
                                {
                                    continue;
                                }

                                if (fisher.Value.tournamentPoints > topScore)
                                {
                                    topScore = fisher.Value.tournamentPoints;
                                    winner = fisher.Value.username;
                                }
                                fisher.Value.tournamentPoints = 0;

                            }
                            if (numParticipants > 0)
                            {
                                irc.sendChatMessage("The fishing tournament has ended! Out of " + numParticipants + " participants, " + winner + " won with " + topScore + " points!");
                                Console.WriteLine("Fishing tournament ended at: " + DateTime.Now.ToString() + ". Winner: " + winner + ". Top Score: " + topScore + ".");

                                repoManager.TournamentResults.Create(new TournamentResult(entries));
                                repoManager.TournamentResults.Commit();
                            }
                            else
                            {
                                irc.sendChatMessage("Fishing tournament has ended.");
                            }
                        }
                    }

                    // iterate through fishing players, see if anyone's got a bite
                    foreach (var player in wolfcoins.fishingList)
                    {
                        if (player.Value.fishHooked)
                        {
                            if ((DateTime.Now - player.Value.timeSinceHook).Seconds > 30)
                            {
                                player.Value.fishHooked = false;
                                player.Value.hookedFishID = -1;

                                Whisper(player.Value.username, "Heck! The fish got away. Maybe next time...", group);
                            }
                        }
                        if (player.Value.isFishing)
                        {
                            if (player.Value.timeOfCatch < DateTime.Now)
                            {
                                player.Value.isFishing = false;
                                player.Value.fishHooked = true;

                                // pick a fish to bite

                                Random rng = new Random();
                                int randFish = WeightedRandomFish(ref fishDatabase).ID;
                                Fish temp = new Fish();
                                foreach (var fish in fishDatabase)
                                {
                                    if (fish.ID == randFish)
                                    {
                                        temp = new Fish(fish);
                                    }
                                }
                                player.Value.hookedFishID = randFish;
                                string toSend = "";

                                switch (temp.sizeCategory)
                                {
                                    case Fish.SIZE_TINY:
                                        {
                                            toSend = "You feel a light tug at your line! Type !catch to reel it in!";
                                        }
                                        break;

                                    case Fish.SIZE_SMALL:
                                        {
                                            toSend = "Something nibbles at your bait! Type !catch to reel it in!";
                                        }
                                        break;

                                    case Fish.SIZE_MEDIUM:
                                        {
                                            toSend = "A strong tug snags your bait! Type !catch to reel it in!";
                                        }
                                        break;

                                    case Fish.SIZE_LARGE:
                                        {
                                            toSend = "Whoa! Something big grabs your line! Type !catch to reel it in!";
                                        }
                                        break;

                                    case Fish.SIZE_HUGE:
                                        {
                                            toSend = "You're almost pulled into the water! Something HUGE is hooked! Type !catch to reel it in!";
                                        }
                                        break;

                                    default: break;
                                }

                                Whisper(player.Value.username, toSend, group);
                                player.Value.timeSinceHook = DateTime.Now;
                            }
                        }
                    }
                    List<int> partiesToRemove = new List<int>();

                    foreach (var party in parties)
                    {
                        if (party.Value.status == PARTY_STARTED && party.Value.myDungeon.messenger.messageQueue.Count() > 0)
                        {

                            if (party.Value.myDungeon.messenger.processQueue() == 1)
                            {
                                // grant rewards here
                                foreach (var member in party.Value.members)
                                {

                                    // if player had an active pet, lower its hunger and affection
                                    if (member.myPets.Count > 0)
                                    {
                                        bool petUpdated = false;
                                        foreach (var pet in wolfcoins.classList[member.name].myPets)
                                        {
                                            // if we updated the active pet already (should only be one), we're done
                                            if (petUpdated)
                                                break;

                                            // check for active pet
                                            if (pet.isActive)
                                            {
                                                Random RNG = new Random();
                                                int hungerToLose = RNG.Next(Pet.DUNGEON_HUNGER, Pet.DUNGEON_HUNGER + 6);

                                                pet.affection -= Pet.DUNGEON_AFFECTION;
                                                pet.hunger -= hungerToLose;

                                                if (pet.hunger <= 0)
                                                {
                                                    // PET DIES HERE
                                                    Whisper(member.name, pet.name + " starved to death.", group);
                                                    wolfcoins.classList[member.name].releasePet(pet.stableID);
                                                    wolfcoins.SaveClassData();
                                                    break;
                                                }
                                                else if (pet.hunger <= 10)
                                                {
                                                    Whisper(member.name, pet.name + " is very hungry and will die if you don't feed it soon!", group);
                                                }
                                                else if (pet.hunger <= 25)
                                                {
                                                    Whisper(member.name, pet.name + " is hungry! Be sure to !feed them!", group);
                                                }



                                                if (pet.affection < 0)
                                                    pet.affection = 0;

                                                petUpdated = true;
                                            }

                                        }
                                    }

                                    if (member.xpEarned == 0 || member.coinsEarned == 0)
                                        continue;

                                    if ((member.xpEarned + member.coinsEarned) > 0 && member.usedGroupFinder && (DateTime.Now - member.lastDailyGroupFinder).TotalDays >= 1)
                                    {
                                        member.lastDailyGroupFinder = DateTime.Now;
                                        member.xpEarned *= 2;
                                        member.coinsEarned *= 2;
                                        Whisper(member.name, "You earned double rewards for completing a daily Group Finder dungeon! Queue up again in 24 hours to receive the 2x bonus again! (You can whisper me '!daily' for a status.)", group);
                                    }

                                    wolfcoins.AwardXP(member.xpEarned, member.name, group);
                                    wolfcoins.AwardCoins(member.coinsEarned, member.name);
                                    if (member.xpEarned > 0 && member.coinsEarned > 0)
                                        Whisper(member.name, member.name + ", you've earned " + member.xpEarned + " XP and " + member.coinsEarned + " Wolfcoins for completing the dungeon!", group);

                                    if (wolfcoins.classList[member.name].itemEarned != -1)
                                    {
                                        int itemID = GrantItem(wolfcoins.classList[member.name].itemEarned, wolfcoins, member.name, itemDatabase);
                                        Whisper(member.name, "You looted " + itemDatabase[(itemID - 1)].itemName + "!", group);
                                    }
                                    // if a pet is waiting to be awarded
                                    if (wolfcoins.classList[member.name].petEarned != -1)
                                    {

                                        Dictionary<int, Pet> allPets = new Dictionary<int, Pet>(petDatabase);
                                        Pet newPet = GrantPet(member.name, wolfcoins, allPets, irc, group);
                                        if (newPet.stableID != -1)
                                        {
                                            string logPath = "petlog.txt";
                                            string timestamp = DateTime.Now.ToString();
                                            if (newPet.isSparkly)
                                            {
                                                System.IO.File.AppendAllText(logPath, timestamp + ": " + member.name + " found a SPARKLY pet " + newPet.name + "." + Environment.NewLine);
                                            }
                                            else
                                            {
                                                System.IO.File.AppendAllText(logPath, timestamp + ": " + member.name + " found a pet " + newPet.name + "." + Environment.NewLine);
                                            }
                                        }
                                        //if (wolfcoins.classList[member.name].petEarned != -1)
                                        //{
                                        //    List<Pet> toAward = new List<Pet>();
                                        //    bool hasActivePet = false;
                                        //    // figure out the rarity of pet to give and build a list of non-duplicate pets to award
                                        //    int rarity = wolfcoins.classList[member.name].petEarned;
                                        //    foreach (var basePet in petDatabase)
                                        //    {
                                        //        if (basePet.Value.petRarity != rarity)
                                        //            continue;

                                        //        bool alreadyOwned = false;

                                        //        foreach(var pet in wolfcoins.classList[member.name].myPets)
                                        //        {
                                        //            if (pet.isActive)
                                        //                hasActivePet = true;

                                        //            if (pet.ID == basePet.Value.ID)
                                        //                alreadyOwned = true;
                                        //        }

                                        //        if(!alreadyOwned)
                                        //        {
                                        //            toAward.Add(basePet.Value);
                                        //        }
                                        //    }
                                        //    // now that we have a list of eligible pets, randomly choose one from the list to award
                                        //    Pet newPet = new Pet();

                                        //    if(toAward.Count > 0)
                                        //    {
                                        //        string toSend = "";
                                        //        Random RNG = new Random();
                                        //        int petToAward = RNG.Next(1, toAward.Count);
                                        //        newPet = toAward[petToAward - 1];
                                        //        int sparklyCheck = RNG.Next(1, 100);

                                        //        if (sparklyCheck == 1)
                                        //            newPet.isSparkly = true;

                                        //        newPet.stableID = wolfcoins.classList[member.name].myPets.Count;
                                        //        wolfcoins.classList[member.name].myPets.Count = wolfcoins.classList[member.name].myPets.Count;

                                        //        if (!hasActivePet)
                                        //        {
                                        //            newPet.isActive = true;
                                        //            toSend = "You found your first pet! You now have a pet " + newPet.name + ". Whisper me !pethelp for more info.";
                                        //        }
                                        //        else
                                        //        {
                                        //            toSend = "You found a new pet buddy! You earned a " + newPet.name + " pet!";
                                        //        }

                                        //        if(newPet.isSparkly)
                                        //        {
                                        //            toSend += " WOW! And it's a sparkly version! Luck you!";
                                        //        }

                                        //        wolfcoins.classList[member.name].myPets.Add(newPet);

                                        //        wolfcoins.classList[member.name].petEarned = -1;
                                        //        Whisper(member.name, toSend, group);
                                        //        if (newPet.isSparkly)
                                        //        {
                                        //            Console.WriteLine(DateTime.Now.ToString() + "WOW! " + ": " + member.name + " just found a SPARKLY pet " + newPet.name + "!");
                                        //            irc.sendChatMessage("WOW! " + member.name + " just found a SPARKLY pet " + newPet.name + "! What luck!");
                                        //        }
                                        //        else
                                        //        {
                                        //            Console.WriteLine(DateTime.Now.ToString() + ": " + member.name + " just found a pet " + newPet.name + "!");
                                        //            irc.sendChatMessage(member.name + " just found a pet " + newPet.name + "!");
                                        //        }
                                        //    }
                                        //}
                                    }

                                    if (wolfcoins.classList[member.name].queueDungeons.Count > 0)
                                        wolfcoins.classList[member.name].ClearQueue();

                                }

                                party.Value.PostDungeon(wolfcoins);
                                wolfcoins.SaveClassData();
                                wolfcoins.SaveXP();
                                wolfcoins.SaveCoins();
                                party.Value.status = PARTY_READY;

                                if (party.Value.usedDungeonFinder)
                                {
                                    partiesToRemove.Add(party.Key);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < partiesToRemove.Count; i++)
                    {
                        int Key = partiesToRemove[i];
                        foreach (var member in parties[Key].members)
                        {
                            Whisper(member.name, "You completed a group finder dungeon. Type !queue to join another group!", group);
                            wolfcoins.classList[member.name].groupID = -1;
                            wolfcoins.classList[member.name].numInvitesSent = 0;
                            wolfcoins.classList[member.name].isPartyLeader = false;
                            wolfcoins.classList[member.name].ClearQueue();
                        }
                        parties.Remove(Key);
                    }

                    if (((DateTime.Now - awardLast).TotalMinutes > awardInterval))
                    {
                        if (broadcasting)
                        {
                            awardTotal = awardAmount * awardMultiplier;
                            wolfcoins.UpdateViewers(channel);

                            // Halloween Treats
                            //Random rnd = new Random();
                            //int numViewers = wolfcoins.viewers.chatters.viewers.Count;
                            //int winner = rnd.Next(0, (numViewers - 1));
                            //string winnerName = wolfcoins.viewers.chatters.viewers.ElementAt(winner);
                            //int coinsToAward = (rnd.Next(5, 10)) * 50;
                            //wolfcoins.AddCoins(winnerName, coinsToAward.ToString());

                            wolfcoins.AwardCoins(awardTotal * 3); // Give 3x as many coins as XP
                            wolfcoins.AwardXP(awardTotal, group);
                            //string path2 = "C:/Users/Lobos/AppData/Roaming/DarkSoulsII/01100001004801af/`s" + DateTime.Now.Ticks + ".sl2";
                            //File.Copy(@"C:\Users\Lobos\AppData\Roaming\DarkSoulsII\01100001004801af\DS2SOFS0000.sl2", @path2);
                            //string path = "C:/Users/Lobos/AppData/Roaming/DarkSoulsIII/01100001004801af/Backups/DS30000_" + DateTime.Now.Ticks + ".sl2";
                            //File.Copy(@"C:/Users/Lobos/AppData/Roaming/DarkSoulsIII/01100001004801af/DS30000.sl2", @path);
                            irc.sendChatMessage("Thanks for watching! Viewers awarded " + awardTotal + " XP & " + (awardTotal * 3) + " Wolfcoins. Subscribers earn double that amount!");
                            //irc.sendChatMessage("Happy Halloween! Viewer " + winnerName + " just won a treat of " + coinsToAward + " wolfcoins!");
                        }

                        wolfcoins.SaveCoins();
                        wolfcoins.SaveXP();
                        wolfcoins.SaveClassData();
                        awardLast = DateTime.Now;
                    }

                    #region whisperRegion
                    if (whispers.Length > 1)
                    {
                        if (whispers[0] != null && whispers[1] != null)
                        {

                            whisperSender = whispers[0];
                            whisperMessage = whispers[1];


                            if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                            {
                                if (wolfcoins.determineLevel(whisperSender) >= 3 && wolfcoins.determineClass(whisperSender) == "INVALID CLASS" && !whisperMessage.StartsWith("c") && !whisperMessage.StartsWith("C"))
                                {
                                    Whisper(whisperSender, "ATTENTION! You are high enough level to pick a class, but have not picked one yet! Whisper me one of the following to choose your class: ", group);
                                    Whisper(whisperSender, "'C1' (Warrior), 'C2' (Mage), 'C3' (Rogue), 'C4' (Ranger), or 'C5' (Cleric)", group);
                                }
                            }
                            if (whisperMessage.StartsWith("!"))
                            {
                                var result = commandManager.ProcessMessage(whisperMessage.Substring(1), whisperSender);
                                if (result != null && result.Processed)
                                {
                                    if (result.Responses != null && result.Responses.Count > 0)
                                    {
                                        foreach (var response in result.Responses)
                                        {
                                            Whisper(whisperSender, response, group);
                                        }
                                    }
                                    if (result.Errors != null && result.Errors.Count > 0)
                                    {
                                        Console.WriteLine($"Errors encountered while processing command \"{whisperMessage}\" from user {whisperSender}");
                                        foreach (var error in result.Errors)
                                        {
                                            Console.WriteLine(error);
                                        }
                                    }
                                    continue;
                                }
                            }

                            if (whisperMessage == "?" || whisperMessage == "help" || whisperMessage == "!help" || whisperMessage == "faq" || whisperMessage == "!faq")
                            {
                                //Whisper(whisperSender, "Help command coming soon. For now, know that only viewers Level 2 & higher can post hyperlinks. This helps keep chat free of bots!");

                                Whisper(whisperSender, "Hi I'm LobotJR! I'm a chat bot written by LobosJR to help out with things.  To ask me about a certain topic, whisper me the number next to what you want to know about! (Ex: Whisper me 1 for information on Wolfcoins)", group);
                                Whisper(whisperSender, "Here's a list of things you can ask me about: Wolfcoins (1) - Leveling System (2)", group);

                            }
                            else if (whisperMessage == "!cleartesters" && (whisperSender == tokenData.BroadcastUser || whisperSender == tokenData.ChatUser))
                            {
                                //string[] users = { "lobosjr", "spectrumknight", "floogoss", "shoumpaloumpa", "nemesis_of_green", "donotgogently", "twitchmage", "kidgreen4", "cuddling", "androsv", "jaranous94", "lambchop2559", "hockeyboy1257", "dumj00", "stennisberetheon", "bionicmeech", "blargh201", "arampizzatime"};
                                //for(int i = 0; i < users.Length; i++)
                                //{
                                //    if (wolfcoins.Exists(wolfcoins.classList, users[i]))
                                //    {
                                //        wolfcoins.classList.Remove(users[i]);
                                //        wolfcoins.SetXP(1, users[i], group);
                                //        wolfcoins.SetXP(600, users[i], group);
                                //    }
                                //    else
                                //    {
                                //        wolfcoins.SetXP(1, users[i], group);
                                //        wolfcoins.SetXP(600, users[i], group);
                                //    }
                                //}

                            }
                            else if (whisperMessage.StartsWith("!dungeon"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    string[] msgData = whispers[1].Split(' ');
                                    int dungeonID = -1;
                                    if (msgData.Count() > 1)
                                    {
                                        int.TryParse(msgData[1], out dungeonID);
                                        if (dungeonID != -1 && dungeonList.ContainsKey(dungeonID))
                                        {
                                            string dungeonPath = "content/dungeons/" + dungeonList[dungeonID];
                                            Dungeon tempDungeon = new Dungeon(dungeonPath);
                                            Whisper(whisperSender, tempDungeon.dungeonName + " (Levels " + tempDungeon.minLevel + " - " + tempDungeon.maxLevel + ") -- " + tempDungeon.description, group);
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "Invalid Dungeon ID provided.", group);
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!bug"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    string[] msgData = whispers[1].Split(' ');
                                    if (msgData.Count() > 1)
                                    {
                                        string bugMessage = "";
                                        for (int i = 1; i < msgData.Count(); i++)
                                        {
                                            bugMessage += msgData[i] + " ";
                                        }

                                        string logPath = "bugreports.log";
                                        System.IO.File.AppendAllText(logPath, whisperSender + ": " + bugMessage + Environment.NewLine);
                                        System.IO.File.AppendAllText(logPath, "------------------------------------------" + Environment.NewLine);

                                        Whisper(whisperSender, "Bug report submitted.", group);
                                        Whisper(tokenData.BroadcastUser, DateTime.Now + ": " + whisperSender + " submitted a bug report.", group);
                                        Console.WriteLine(DateTime.Now + ": " + whisperSender + " submitted a bug report.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!item"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myItems.Count == 0)
                                    {
                                        Whisper(whisperSender, "You have no items.", group);
                                        continue;
                                    }

                                    string[] msgData = whispers[1].Split(' ');
                                    int invID = -1;
                                    if (msgData.Count() > 1)
                                    {
                                        int.TryParse(msgData[1], out invID);
                                        if (invID != -1)
                                        {
                                            bool itemFound = false;
                                            foreach (var item in wolfcoins.classList[whisperSender].myItems)
                                            {
                                                if (item.inventoryID == invID)
                                                {
                                                    string desc = itemDatabase[item.itemID - 1].description;
                                                    string name = itemDatabase[item.itemID - 1].itemName;
                                                    Whisper(whisperSender, name + " -- " + desc, group);
                                                    itemFound = true;
                                                    break;
                                                }
                                            }
                                            if (!itemFound)
                                            {
                                                Whisper(whisperSender, "Invalid Inventory ID provided.", group);
                                            }
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "Invalid Inventory ID provided.", group);
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!updateviewers")
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                {
                                    continue;
                                }

                                wolfcoins.UpdateViewers(channel);
                            }
                            else if (whisperMessage == "!updateitems")
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                UpdateItems(itemListPath, ref itemList, ref itemDatabase);


                                foreach (var player in wolfcoins.classList)
                                {
                                    if (player.Value.myItems.Count == 0)
                                        continue;

                                    foreach (var item in player.Value.myItems)
                                    {
                                        Item newItem = itemDatabase[item.itemID - 1];

                                        item.itemName = newItem.itemName;
                                        item.itemRarity = newItem.itemRarity;
                                        item.itemFind = newItem.itemFind;
                                        item.successChance = newItem.successChance;
                                        item.coinBonus = newItem.coinBonus;
                                        item.preventDeathBonus = newItem.preventDeathBonus;
                                        item.xpBonus = newItem.xpBonus;
                                    }
                                }
                            }
                            else if (whisperMessage == "!godmode")
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                wolfcoins.classList[whisperSender].successChance = 1000;

                            }
                            else if (whisperMessage.StartsWith("!addplayer"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() == 2)
                                {
                                    string name = msgData[1];
                                    string toSend = "";
                                    if (!wolfcoins.classList.ContainsKey(name))
                                    {
                                        wolfcoins.classList.Add(name.ToLower(), new CharClass());
                                        toSend += "class, ";
                                    }

                                    if (!wolfcoins.coinList.ContainsKey(name))
                                    {
                                        wolfcoins.coinList.Add(name, 0);
                                        toSend += "coin, ";
                                    }

                                    if (!wolfcoins.xpList.ContainsKey(name))
                                    {
                                        wolfcoins.xpList.Add(name, 0);
                                        toSend += "xp";
                                    }

                                    Whisper(tokenData.BroadcastUser, name + " added to the following lists: " + toSend, group);
                                }
                            }
                            else if (whisperMessage.StartsWith("!transfer"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() > 2 && msgData.Count() < 4)
                                {
                                    string prevName = msgData[1];
                                    string newName = msgData[2];


                                    if (!wolfcoins.coinList.ContainsKey(prevName) || !wolfcoins.xpList.ContainsKey(prevName))
                                    {
                                        Whisper(whisperSender, prevName + " has no stats to transfer.", group);
                                        continue;
                                    }

                                    if (!wolfcoins.coinList.ContainsKey(newName))
                                    {
                                        wolfcoins.coinList.Add(newName, 0);
                                    }

                                    if (!wolfcoins.xpList.ContainsKey(newName))
                                    {
                                        wolfcoins.xpList.Add(newName, 0);
                                    }

                                    int prevCoins = wolfcoins.coinList[prevName];
                                    int prevXP = wolfcoins.xpList[prevName];

                                    wolfcoins.coinList[newName] += prevCoins;
                                    wolfcoins.xpList[newName] += prevXP;

                                    if (!wolfcoins.classList.ContainsKey(newName))
                                    {
                                        CharClass playerClass = new CharClass();
                                        wolfcoins.classList.Add(newName.ToLower(), new CharClass());
                                    }

                                    Whisper(whisperSender, "Transferred " + prevName + "'s xp/coins to " + newName + ".", group);
                                    Whisper(newName, "Your xp/coin total has been updated by Lobos! Thanks for playing the RPG lobosHi", group);

                                    wolfcoins.SaveCoins();
                                    wolfcoins.SaveXP();

                                }
                            }
                            else if (whisperMessage.StartsWith("!checkpets"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() > 1)
                                {
                                    string toCheck = msgData[1];
                                    foreach (var pet in wolfcoins.classList[toCheck].myPets)
                                    {
                                        WhisperPet(whisperSender, pet, group, LOW_DETAIL);
                                    }

                                }
                                else
                                {
                                    Whisper(whisperSender, "!checkpets <username>", group);
                                }
                            }
                            else if (whisperMessage.StartsWith("!grantpet"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() > 1)
                                {
                                    int rarity = -1;
                                    if (int.TryParse(msgData[1], out rarity))
                                    {
                                        wolfcoins.classList[whisperSender].petEarned = rarity;
                                    }
                                }
                                else
                                {
                                    Random rng = new Random();
                                    wolfcoins.classList[whisperSender].petEarned = rng.Next(1, 6);
                                }
                                Dictionary<int, Pet> allPets = petDatabase;
                                GrantPet(whisperSender, wolfcoins, allPets, irc, group);
                                //Random RNG = new Random();

                                //Pet newPet = new Pet();

                                //int petToAward = RNG.Next(1, petDatabase.Count);
                                //newPet = petDatabase[petToAward];
                                //int sparklyCheck = RNG.Next(1, 100);

                                //if (sparklyCheck == 1)
                                //    newPet.isSparkly = true;

                                //wolfcoins.classList[whisperSender].myPets.Count++;
                                //newPet.stableID = wolfcoins.classList[whisperSender].myPets.Count;

                                //wolfcoins.classList[whisperSender].myPets.Add(newPet);

                                //Whisper(whisperSender, "Added a random pet.", group);

                            }
                            else if (whisperMessage == "!clearpets")
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                wolfcoins.classList[whisperSender].myPets = new List<Pet>();
                                wolfcoins.classList[whisperSender].toRelease = new Pet();
                                wolfcoins.classList[whisperSender].pendingPetRelease = false;

                                Whisper(whisperSender, "Pets cleared.", group);

                            }
                            else if (whisperMessage == "!updatedungeons")
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                UpdateDungeons(dungeonListPath, ref dungeonList);
                            }
                            else if (whisperMessage.StartsWith("/p") || whisperMessage.StartsWith("/party"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].groupID != -1)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() > 1)
                                        {
                                            string partyMessage = "";
                                            for (int i = 1; i < msgData.Count(); i++)
                                            {
                                                partyMessage += msgData[i] + " ";
                                            }
                                            int partyID = wolfcoins.classList[whisperSender].groupID;
                                            foreach (var member in parties[partyID].members)
                                            {
                                                if (member.name == whisperSender)
                                                {
                                                    Whisper(member.name, "You whisper: \" " + partyMessage + "\" ", group);
                                                    continue;
                                                }

                                                Whisper(member.name, whisperSender + " says: \" " + partyMessage + "\" ", group);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!respec")
                            {
                                if (wolfcoins.classList != null)
                                {
                                    if (wolfcoins.classList.Keys.Contains(whisperSender.ToLower()) && wolfcoins.determineClass(whisperSender) != "INVALID_CLASS")
                                    {
                                        if (wolfcoins.classList[whisperSender].groupID == -1)
                                        {
                                            if (wolfcoins.Exists(wolfcoins.coinList, whisperSender))
                                            {

                                                int respecCost = (baseRespecCost * (wolfcoins.classList[whisperSender].level - 4));
                                                if (respecCost < baseRespecCost)
                                                    respecCost = baseRespecCost;

                                                if (wolfcoins.coinList[whisperSender] <= respecCost)
                                                {
                                                    Whisper(whisperSender, "It costs " + respecCost + " Wolfcoins to respec at your level. You have " + wolfcoins.coinList[whisperSender] + " coins.", group);
                                                }
                                                int classNumber = wolfcoins.classList[whisperSender].classType * 10;
                                                wolfcoins.classList[whisperSender].classType = classNumber;

                                                Whisper(whisperSender, "You've chosen to respec your class! It will cost you " + respecCost + " coins to respec and you will lose all your items. Reply 'Nevermind' to cancel or one of the following codes to select your new class: ", group);
                                                Whisper(whisperSender, "'C1' (Warrior), 'C2' (Mage), 'C3' (Rogue), 'C4' (Ranger), or 'C5' (Cleric)", group);
                                            }
                                            else
                                            {
                                                Whisper(whisperSender, "You have no coins to respec with.", group);
                                            }
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "You can't respec while in a party!", group);
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!inventory" || whisperMessage == "!inv" || whisperMessage == "inv" || whisperMessage == "inventory")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myItems.Count > 0)
                                    {
                                        Whisper(whisperSender, "You have " + wolfcoins.classList[whisperSender].myItems.Count + " items: ", group);
                                        foreach (var item in wolfcoins.classList[whisperSender].myItems)
                                        {
                                            WhisperItem(whisperSender, item, group, itemDatabase);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You have no items.", group);
                                    }
                                }
                            }
                            else if (whisperMessage == "!fishleaders" || whisperMessage == "!leaderboards")
                            {
                                foreach (var fish in wolfcoins.fishingLeaderboard)
                                {
                                    Whisper(whisperSender, "Largest " + fish.name + " caught by " + fish.caughtBy + " at " + fish.weight + " lbs.", group);
                                }
                            }
                            else if (whisperMessage == "!fishleaders -c" || whisperMessage == "!leaderboards -c")
                            {
                                var compact = wolfcoins.fishingLeaderboard.Select(x => $"{x.name}|{x.length}|{x.weight}|{x.caughtBy};");
                                var prefix = "fishleaders: ";
                                var toSend = prefix;
                                foreach (var entry in compact)
                                {
                                    if (toSend.Length + entry.Length > 450)
                                    {
                                        Whisper(whisperSender, toSend, group);
                                        toSend = prefix;
                                    }
                                    toSend += entry;
                                }
                                Whisper(whisperSender, toSend, group);
                            }
                            else if (whisperMessage == "!fish")
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                                    {
                                        Whisper(whisperSender, "You've caught " + wolfcoins.fishingList[whisperSender].biggestFish.Count + " different types of fish: ", group);
                                        WhisperFish(whisperSender, wolfcoins.fishingList[whisperSender].biggestFish, group);

                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You haven't caught any fish yet!", group);
                                    }
                                }
                            }
                            else if (whisperMessage == "!fish -c")
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    var compact = wolfcoins.fishingList[whisperSender].biggestFish.Select(x => $"{x.name}|{x.length}|{x.weight};");
                                    var prefix = "fish: ";
                                    var toSend = prefix;
                                    foreach (var entry in compact)
                                    {
                                        if (toSend.Length + entry.Length > 450)
                                        {
                                            Whisper(whisperSender, toSend, group);
                                            toSend = prefix;
                                        }
                                        toSend += entry;
                                    }
                                    Whisper(whisperSender, toSend, group);
                                }
                            }
                            else if (whisperMessage.StartsWith("!fish"))
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !fish <Fish #>", group);
                                            continue;
                                        }
                                        int fishID = -1;
                                        if (int.TryParse(msgData[1], out fishID))
                                        {
                                            if (fishID <= wolfcoins.fishingList[whisperSender].biggestFish.Count && fishID > 0)
                                            {
                                                WhisperFish(whisperSender, wolfcoins.fishingList, fishID, group);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have any fish! Type !cast to try and fish for some!", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!releasefish"))
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !fish <Fish #>", group);
                                            continue;
                                        }
                                        int fishID = -1;
                                        if (int.TryParse(msgData[1], out fishID))
                                        {
                                            if (fishID <= wolfcoins.fishingList[whisperSender].biggestFish.Count && fishID > 0)
                                            {
                                                string fishName = wolfcoins.fishingList[whisperSender].biggestFish[fishID - 1].name;
                                                wolfcoins.fishingList[whisperSender].biggestFish.RemoveAt(fishID - 1);

                                                Whisper(whisperSender, "You released your " + fishName + ". Bye bye!", group);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have any fish! Type !cast to try and fish for some!", group);
                                    }
                                }
                            }
                            else if (whisperMessage == "!pets" || whisperMessage == "!stable" || whisperMessage == "pets" || whisperMessage == "stable")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        Whisper(whisperSender, "You have " + wolfcoins.classList[whisperSender].myPets.Count + " pets: ", group);
                                        foreach (var pet in wolfcoins.classList[whisperSender].myPets)
                                        {
                                            WhisperPet(whisperSender, pet, group, LOW_DETAIL);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You have no pets.", group);
                                    }
                                }
                            }

                            else if (whisperMessage == "!pethelp")
                            {
                                Whisper(whisperSender, "View all your pets by whispering me '!pets'. View individual pet stats using '!pet <stable id>' where the id is the number next to your pet's name in brackets [].", group);
                                Whisper(whisperSender, "A summoned/active pet will join you on dungeon runs and possibly even bring benefits! But this will drain its energy, which you can restore by feeding it.", group);
                                Whisper(whisperSender, "You can !dismiss, !summon, !release, !feed, and !hug* your pets using their stable id (ex: !summon 2)", group);
                                Whisper(whisperSender, "*: In development, available soon!", group);
                            }
                            else if (whisperMessage.StartsWith("!fixpets"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                //string[] msgData = whispers[1].Split(' ');
                                //if (msgData.Count() != 2)
                                //{
                                //    Whisper(whisperSender, "Invalid number of parameters. Syntax: !feed <stable ID>", group);
                                //    continue;
                                //}

                                //string playerToFix = msgData[1];
                                foreach (var player in wolfcoins.classList)
                                {
                                    if (player.Value.myPets.Count == 0)
                                    {
                                        continue;
                                    }
                                    int stableIDFix = 1;
                                    foreach (var pet in player.Value.myPets)
                                    {
                                        pet.stableID = stableIDFix;
                                        stableIDFix++;
                                    }
                                    Whisper(whisperSender, "Fixed " + player.Value.name + "'s pet IDs.", group);
                                }
                            }
                            else if (whisperMessage.StartsWith("!feed"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && wolfcoins.Exists(wolfcoins.coinList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !feed <stable ID>", group);
                                            continue;
                                        }
                                        int petToFeed = -1;
                                        if (int.TryParse(msgData[1], out petToFeed))
                                        {

                                            if (petToFeed > wolfcoins.classList[whisperSender].myPets.Count || petToFeed < 1)
                                            {
                                                Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!", group);
                                                continue;
                                            }

                                            if (wolfcoins.coinList[whisperSender] < 5)
                                            {
                                                Whisper(whisperSender, "You lack the 5 wolfcoins to feed your pet! Hop in a Lobos stream soon!", group);
                                                continue;
                                            }

                                            // build a dummy pet to do calculations
                                            Pet tempPet = wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1);

                                            if (tempPet.hunger >= Pet.HUNGER_MAX)
                                            {
                                                Whisper(whisperSender, tempPet.name + " is full and doesn't need to eat!", group);
                                                continue;
                                            }

                                            int currentHunger = tempPet.hunger;
                                            int currentXP = tempPet.xp;
                                            int currentLevel = tempPet.level;
                                            int currentAffection = tempPet.affection + Pet.FEEDING_AFFECTION;

                                            // Charge the player for pet food
                                            wolfcoins.coinList[whisperSender] = wolfcoins.coinList[whisperSender] - Pet.FEEDING_COST;

                                            Whisper(whisperSender, "You were charged " + Pet.FEEDING_COST + " wolfcoins to feed " + tempPet.name + ". They feel refreshed!", group);
                                            // earn xp equal to amount of hunger 'fed'
                                            currentXP += (Pet.HUNGER_MAX - currentHunger);

                                            // check if pet leveled
                                            if (currentXP >= Pet.XP_TO_LEVEL && currentLevel < Pet.LEVEL_MAX)
                                            {
                                                currentLevel++;
                                                currentXP = currentXP - Pet.XP_TO_LEVEL;
                                                Whisper(whisperSender, tempPet.name + " leveled up! They are now level " + currentLevel + ".", group);
                                            }
                                            // refill hunger value
                                            currentHunger = Pet.HUNGER_MAX;

                                            // update temp pet w/ new data
                                            tempPet.affection = currentAffection;
                                            tempPet.hunger = currentHunger;
                                            tempPet.xp = currentXP;
                                            tempPet.level = currentLevel;

                                            // update actual pet data
                                            wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1).affection = currentAffection;
                                            wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1).hunger = currentHunger;
                                            wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1).xp = currentXP;
                                            wolfcoins.classList[whisperSender].myPets.ElementAt(petToFeed - 1).level = currentLevel;

                                            wolfcoins.SaveClassData();
                                            wolfcoins.SaveCoins();
                                        }

                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!sethunger"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] msgData = whispers[1].Split(' ');
                                if (msgData.Count() > 3)
                                {
                                    Whisper(whisperSender, "Too many parameters. Syntax: !sethunger <stable ID>", group);
                                    continue;
                                }
                                int petToSet = -1;
                                int amount = -1;
                                if (int.TryParse(msgData[1], out petToSet) && int.TryParse(msgData[2], out amount))
                                {
                                    wolfcoins.classList[whisperSender].myPets.ElementAt(petToSet - 1).hunger = amount;
                                    Whisper(whisperSender, wolfcoins.classList[whisperSender].myPets.ElementAt(petToSet - 1).name + "'s energy set to " + amount + ".", group);
                                }
                                else
                                {
                                    Whisper(whisperSender, "Ya dun fucked somethin' up.", group);
                                }
                            }
                            else if (whisperMessage.StartsWith("!release"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !release <stable ID>", group);
                                            continue;
                                        }
                                        int petToRelease = -1;
                                        if (int.TryParse(msgData[1], out petToRelease))
                                        {

                                            if (petToRelease > wolfcoins.classList[whisperSender].myPets.Count || petToRelease < 1)
                                            {
                                                Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!", group);
                                                continue;
                                            }
                                            string petName = wolfcoins.classList[whisperSender].myPets.ElementAt(petToRelease - 1).name;
                                            //wolfcoins.classList[whisperSender].toRelease = petToRelease;
                                            wolfcoins.classList[whisperSender].pendingPetRelease = true;
                                            wolfcoins.classList[whisperSender].toRelease = new Pet();
                                            wolfcoins.classList[whisperSender].toRelease.stableID = wolfcoins.classList[whisperSender].myPets.ElementAt(petToRelease - 1).stableID;
                                            Whisper(whisperSender, "If you release " + petName + ", they will be gone forever. Are you sure you want to release them? (y/n)", group);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have a pet.", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!dismiss"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !dismiss <stable ID>", group);
                                            continue;
                                        }
                                        int petToDismiss = -1;
                                        if (int.TryParse(msgData[1], out petToDismiss))
                                        {
                                            if (petToDismiss > wolfcoins.classList[whisperSender].myPets.Count || petToDismiss < 1)
                                            {
                                                Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!", group);
                                                continue;
                                            }
                                            if (wolfcoins.classList[whisperSender].myPets.ElementAt(petToDismiss - 1).isActive)
                                            {
                                                wolfcoins.classList[whisperSender].myPets.ElementAt(petToDismiss - 1).isActive = false;
                                                Whisper(whisperSender, "You dismissed " + wolfcoins.classList[whisperSender].myPets.ElementAt(petToDismiss - 1).name + ".", group);
                                                wolfcoins.SaveClassData();
                                            }
                                            else
                                            {
                                                Whisper(whisperSender, "That pet is not currently summoned.", group);
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have a pet.", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!summon"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !summon <stable ID>", group);
                                            continue;
                                        }
                                        int petToSummon = -1;
                                        int currentlyActivePet = -1;
                                        if (int.TryParse(msgData[1], out petToSummon))
                                        {
                                            if (petToSummon > wolfcoins.classList[whisperSender].myPets.Count || petToSummon < 1)
                                            {
                                                Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!", group);
                                                continue;
                                            }
                                            foreach (var pet in wolfcoins.classList[whisperSender].myPets)
                                            {
                                                if (pet.isActive)
                                                {
                                                    currentlyActivePet = pet.stableID;
                                                }
                                            }
                                            if (currentlyActivePet > wolfcoins.classList[whisperSender].myPets.Count)
                                            {
                                                Whisper(whisperSender, "Sorry, your stableID is corrupt. Lobos is working on this issue :(", group);
                                                continue;
                                            }
                                            if (!wolfcoins.classList[whisperSender].myPets.ElementAt(petToSummon - 1).isActive)
                                            {
                                                wolfcoins.classList[whisperSender].myPets.ElementAt(petToSummon - 1).isActive = true;
                                                Whisper(whisperSender, "You summoned " + wolfcoins.classList[whisperSender].myPets.ElementAt(petToSummon - 1).name + ".", group);
                                                if (currentlyActivePet != -1)
                                                {
                                                    wolfcoins.classList[whisperSender].myPets.ElementAt(currentlyActivePet - 1).isActive = false;
                                                    Whisper(whisperSender, wolfcoins.classList[whisperSender].myPets.ElementAt(currentlyActivePet - 1).name + " was dismissed.", group);
                                                }
                                            }
                                            else
                                            {
                                                Whisper(whisperSender, wolfcoins.classList[whisperSender].myPets.ElementAt(petToSummon - 1).name + " is already summoned!", group);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have a pet.", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!pet"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !pet <stable ID>", group);
                                            continue;
                                        }
                                        int petToSend = -1;
                                        if (int.TryParse(msgData[1], out petToSend))
                                        {
                                            if (petToSend > wolfcoins.classList[whisperSender].myPets.Count || petToSend < 1)
                                            {
                                                Whisper(whisperSender, "Invalid Stable ID given. Check !pets for each pet's stable ID!", group);
                                                continue;
                                            }
                                            WhisperPet(whisperSender, wolfcoins.classList[whisperSender].myPets.ElementAt(petToSend - 1), group, HIGH_DETAIL);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have any pets.", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!rename"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 3)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Note: names cannot contain spaces.", group);
                                            continue;
                                        }
                                        else if (msgData.Count() == 3)
                                        {
                                            int petToRename = -1;
                                            if (int.TryParse(msgData[1], out petToRename))
                                            {
                                                if (petToRename > (wolfcoins.classList[whisperSender].myPets.Count) || petToRename < 1)
                                                {
                                                    Whisper(whisperSender, "Sorry, the Stable ID given was invalid. Please try again.", group);
                                                    continue;
                                                }
                                                string newName = msgData[2];
                                                if (newName.Length > 16)
                                                {
                                                    Whisper(whisperSender, "Name can only be 16 characters max.", group);
                                                    continue;
                                                }
                                                string prevName = wolfcoins.classList[whisperSender].myPets.ElementAt(petToRename - 1).name;
                                                wolfcoins.classList[whisperSender].myPets.ElementAt(petToRename - 1).name = newName;
                                                Whisper(whisperSender, prevName + " was renamed to " + newName + "!", group);
                                            }
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "Sorry, the data you provided didn't work. Syntax: !rename <stable id> <new name>", group);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have any pets to rename. :(", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!cancelcast"))
                            {
                                if ((wolfcoins.Exists(wolfcoins.fishingList, whisperSender)))
                                {
                                    if (wolfcoins.fishingList[whisperSender].isFishing)
                                    {
                                        wolfcoins.fishingList[whisperSender].isFishing = false;
                                        wolfcoins.fishingList[whisperSender].fishHooked = false;
                                        wolfcoins.fishingList[whisperSender].hookedFishID = -1;
                                        Whisper(whisperSender, "You reel in the empty line.", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!catch") || whisperMessage.StartsWith("!reel"))
                            {
                                if ((wolfcoins.Exists(wolfcoins.fishingList, whisperSender)))
                                {
                                    if (wolfcoins.fishingList[whisperSender].isFishing && (!wolfcoins.fishingList[whisperSender].fishHooked))
                                    {
                                        Whisper(whisperSender, "Nothing is biting yet! To reset your cast, use !cancelcast", group);
                                    }

                                    if (wolfcoins.fishingList[whisperSender].fishHooked && wolfcoins.fishingList[whisperSender].hookedFishID != -1)
                                    {
                                        Fish myCatch = new Fish();

                                        foreach (var fish in fishDatabase)
                                        {
                                            if (fish.ID == wolfcoins.fishingList[whisperSender].hookedFishID)
                                            {
                                                myCatch = (wolfcoins.fishingList[whisperSender].Catch(new Fish(fish), group, fishingTournamentActive, wolfcoins.fishingList));
                                            }
                                        }

                                        if (fishingTournamentActive)
                                        {
                                            // update leaderboard
                                            bool matchFound = false;
                                            for (int i = 0; i < wolfcoins.fishingLeaderboard.Count; i++)
                                            {
                                                if (wolfcoins.fishingLeaderboard.ElementAt(i).ID == myCatch.ID)
                                                {
                                                    matchFound = true;
                                                    if (myCatch.weight > wolfcoins.fishingLeaderboard.ElementAt(i).weight)
                                                    {
                                                        wolfcoins.fishingLeaderboard[i] = new Fish(myCatch);
                                                        irc.sendChatMessage(whisperSender + " just caught the heaviest " + myCatch.name + " ever! It weighs " + myCatch.weight + " pounds!");
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!matchFound)
                                            {
                                                wolfcoins.fishingLeaderboard.Add(new Fish(myCatch));
                                                irc.sendChatMessage(whisperSender + " just caught the heaviest " + myCatch.name + " ever! It weighs " + myCatch.weight + " pounds!");

                                            }

                                            wolfcoins.SaveFishingList();
                                        }
                                        if (!fishingTournamentActive)
                                        {
                                            Whisper(whisperSender, "Congratulations! You caught a " + myCatch.length + " inch, " +
                                                myCatch.weight + " pound " + myCatch.name + "!", group);
                                        }

                                        wolfcoins.fishingList[whisperSender].fishHooked = false;
                                        wolfcoins.fishingList[whisperSender].hookedFishID = -1;



                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!cast"))
                            {
                                if (!(wolfcoins.Exists(wolfcoins.fishingList, whisperSender)))
                                {
                                    // first time fishing! initialize all necessary values
                                    Fisherman temp = new Fisherman();
                                    temp.username = whisperSender;
                                    temp.level = 1;
                                    temp.XP = 0;
                                    temp.lure = 0;

                                    // add new fisherman and save
                                    wolfcoins.fishingList.Add(whisperSender, temp);
                                    wolfcoins.SaveFishingList();

                                }
                                if (wolfcoins.fishingList[whisperSender].isFishing)
                                {
                                    Whisper(whisperSender, "Your line is already cast! I'm sure a fish'll be along soon...", group);
                                    continue;
                                }

                                if (wolfcoins.fishingList[whisperSender].fishHooked)
                                {
                                    Whisper(whisperSender, "Something's already bit your line! Quick, type !catch to snag it!", group);
                                    continue;
                                }

                                // min/max time, in seconds, before a fish will bite
                                int minimumCastTime = 60;
                                int maximumCastTime = 600;

                                if (fishingTournamentActive)
                                {
                                    minimumCastTime = tournamentCastTimeMax / 2;
                                    maximumCastTime = tournamentCastTimeMax;
                                }
                                // determine when a fish will bite
                                Random rng = new Random();
                                int elapsedTime = rng.Next(minimumCastTime, maximumCastTime);

                                wolfcoins.fishingList[whisperSender].timeOfCatch = DateTime.Now.AddSeconds(elapsedTime);
                                wolfcoins.fishingList[whisperSender].isFishing = true;

                                Whisper(whisperSender, "You cast your line out into the water.", group);
                            }
                            else if (whisperMessage == "!debugcast")
                            {
                                // min/max time, in seconds, before a fish will bite
                                if (whisperSender == tokenData.BroadcastUser || whisperSender == tokenData.ChatUser)
                                {
                                    wolfcoins.fishingList[whisperSender].timeOfCatch = DateTime.Now.AddSeconds(2);
                                    wolfcoins.fishingList[whisperSender].isFishing = true;

                                    Whisper(whisperSender, "You cast your line out into the water.", group);
                                }
                            }
                            else if (whisperMessage == "!nexttournament")
                            {
                                if (!broadcasting)
                                {
                                    Whisper(whisperSender, "Stream is offline. Next fishing tournament will begin 15m after the beginning of next stream.", group);
                                }
                                else
                                {
                                    if (fishingTournamentActive)
                                    {
                                        Whisper(whisperSender, "A fishing tournament is active now! Go catch fish at: https://tinyurl.com/PlayWolfpackRPG !", group);
                                    }
                                    string tourneyTime = "";
                                    tourneyTime += (nextTournament - DateTime.Now).Minutes;
                                    Whisper(whisperSender, "Next fishing tournament begins in " + tourneyTime + " minutes.", group);
                                }
                            }
                            else if (whisperMessage == "!nexttournament -c")
                            {
                                if (broadcasting)
                                {
                                    if (fishingTournamentActive)
                                    {
                                        var maxDuration = new TimeSpan(0, tournamentDuration, 0); // value is in minutes, so convert to seconds to compare against time elapsed
                                        var currentDuration = DateTime.Now - tournamentStart;
                                        var left = maxDuration - currentDuration;
                                        Whisper(whisperSender, $"nexttournament: -{left.ToString("c")}", group);
                                    }
                                    else
                                    {
                                        var toNext = nextTournament - DateTime.Now;
                                        Whisper(whisperSender, $"nexttournament: {toNext.ToString("c")}", group);
                                    }
                                }
                            }
                            else if (whisperMessage == "!debugtournament")
                            {
                                // Enable a fishing tournament! 
                                // set bool to check against | bool fishingTournament = TOURNAMENT_ACTIVE; 
                                fishingTournamentActive = true;
                                // set tourney start time i.e. tournamentStart = DateTime.Now 
                                tournamentStart = DateTime.Now;
                                tournamentDuration = 1;
                                irc.sendChatMessage("A fishing tournament has begun! Participate at: https://tinyurl.com/PlayWolfpackRPG");

                            }
                            else if (whisperMessage.StartsWith("!start"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    int partyID = wolfcoins.classList[whisperSender].groupID;
                                    if (parties.Count() > 0 && partyID != -1 && parties.ContainsKey(partyID))
                                    {
                                        if (parties[partyID].status == PARTY_READY)
                                        {
                                            if (!(parties[partyID].partyLeader == whisperSender))
                                            {
                                                Whisper(whisperSender, "You are not the party leader!", group);
                                                continue;
                                            }
                                            string[] msgData = whispers[1].Split(' ');
                                            int dungeonID = -1;
                                            if (msgData.Count() > 1)
                                            {
                                                int.TryParse(msgData[1], out dungeonID);
                                            }
                                            else if (wolfcoins.classList[whisperSender].groupFinderDungeon != -1)
                                            {
                                                dungeonID = wolfcoins.classList[whisperSender].groupFinderDungeon;
                                                wolfcoins.classList[whisperSender].groupFinderDungeon = -1;
                                            }
                                            //else if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                            //{
                                            //    dungeonID = wolfcoins.classList[whisperSender].queueDungeons.ElementAt(0);
                                            //}
                                            else
                                            {
                                                Whisper(whisperSender, "Invalid Dungeon ID provided.", group);
                                                continue;
                                            }
                                            if (dungeonList.Count() >= dungeonID && dungeonID > 0)
                                            {
                                                string dungeonPath = "content/dungeons/" + dungeonList[dungeonID];
                                                IEnumerable<string> fileText = System.IO.File.ReadLines(dungeonPath, UTF8Encoding.Default);
                                                string[] type = fileText.ElementAt(0).Split('=');
                                                if (type[1] == "Dungeon" && parties[partyID].NumMembers() > 3)
                                                {
                                                    Whisper(whisperSender, "You can't have more than 3 party members for a Dungeon.", group);
                                                    continue;
                                                }

                                                if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                                {
                                                    foreach (var member in parties[partyID].members)
                                                    {
                                                        wolfcoins.classList[member.name].queueDungeons = new List<int>();
                                                    }
                                                }

                                                Dungeon newDungeon = new Dungeon(dungeonPath, channel, itemDatabase);
                                                bool outOfLevelRange = false;
                                                foreach (var member in parties[partyID].members)
                                                {
                                                    member.level = wolfcoins.determineLevel(member.name);
                                                    //if (member.level < newDungeon.minLevel)
                                                    //{
                                                    //    Whisper(parties[partyID], member.name + " is not high enough level for the requested dungeon. (Min Level: " + newDungeon.minLevel + ")", group);
                                                    //    outOfLevelRange = true;
                                                    //}
                                                }
                                                int minLevel = 3;
                                                List<string> brokeBitches = new List<string>();
                                                bool enoughMoney = true;
                                                foreach (var member in parties[partyID].members)
                                                {

                                                    if (wolfcoins.Exists(wolfcoins.coinList, member.name))
                                                    {
                                                        if (wolfcoins.coinList[member.name] < (baseDungeonCost + ((member.level - minLevel) * 10)))
                                                        {
                                                            brokeBitches.Add(member.name);
                                                            enoughMoney = false;
                                                        }
                                                    }
                                                }

                                                if (!enoughMoney)
                                                {
                                                    string names = "";
                                                    foreach (var bitch in brokeBitches)
                                                    {
                                                        names += bitch + " ";
                                                    }
                                                    Whisper(parties[partyID], "The following party members do not have enough money to run " + newDungeon.dungeonName + ": " + names, group);
                                                }

                                                if (!outOfLevelRange && enoughMoney)
                                                {
                                                    foreach (var member in parties[partyID].members)
                                                    {
                                                        wolfcoins.coinList[member.name] -= (baseDungeonCost + ((member.level - minLevel) * 10));
                                                    }
                                                    Whisper(parties[partyID], "Successfully initiated " + newDungeon.dungeonName + "! Wolfcoins deducted.", group);
                                                    string memberInfo = "";
                                                    foreach (var member in parties[partyID].members)
                                                    {
                                                        memberInfo += member.name + " (Level " + member.level + " " + member.className + ") ";
                                                    }

                                                    Whisper(parties[partyID], "Your party consists of: " + memberInfo, group);
                                                    parties[partyID].status = PARTY_STARTED;
                                                    parties[partyID].myDungeon = newDungeon;
                                                    parties[partyID] = parties[partyID].myDungeon.RunDungeon(parties[partyID], ref group);

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "y")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].pendingPetRelease)
                                    {
                                        int toRelease = wolfcoins.classList[whisperSender].toRelease.stableID;
                                        if (toRelease > wolfcoins.classList[whisperSender].myPets.Count)
                                        {
                                            Whisper(whisperSender, "Stable ID mismatch. Try !release again.", group);
                                            continue;
                                        }
                                        string petName = wolfcoins.classList[whisperSender].myPets.ElementAt(toRelease - 1).name;
                                        if (wolfcoins.classList[whisperSender].releasePet(toRelease))
                                        {
                                            Whisper(whisperSender, "You released " + petName + ". Goodbye, " + petName + "!", group);
                                            wolfcoins.SaveClassData();
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "Something went wrong. " + petName + " is still with you!", group);
                                        }
                                        wolfcoins.classList[whisperSender].pendingPetRelease = false;
                                        wolfcoins.classList[whisperSender].toRelease = new Pet();
                                    }

                                    if (wolfcoins.classList[whisperSender].pendingInvite)
                                    {
                                        int partyID = wolfcoins.classList[whisperSender].groupID;
                                        wolfcoins.classList[whisperSender].pendingInvite = false;
                                        string partyLeader = parties[partyID].partyLeader;
                                        int partySize = parties[partyID].NumMembers();
                                        string myClass = wolfcoins.classList[whisperSender].className;
                                        int myLevel = wolfcoins.determineLevel(wolfcoins.xpList[whisperSender]);
                                        string myMembers = "";
                                        foreach (var member in parties[partyID].members)
                                        {
                                            myMembers += member.name + " ";
                                        }
                                        Whisper(whisperSender, "You successfully joined a party with the following members: " + myMembers, group);
                                        foreach (var member in parties[partyID].members)
                                        {
                                            if (member.name == whisperSender)
                                                continue;

                                            if (member.pendingInvite)
                                                continue;

                                            Whisper(member.name, whisperSender + ", Level " + myLevel + " " + myClass + " has joined your party! (" + partySize + "/" + DUNGEON_MAX + ")", group);
                                        }

                                        if (partySize == DUNGEON_MAX)
                                        {
                                            Whisper(partyLeader, "Your party is now full.", group);
                                            parties[partyID].status = PARTY_FULL;
                                        }

                                        if (partySize == 3)
                                        {
                                            Whisper(partyLeader, "You've reached 3 party members! You're ready to dungeon!", group);
                                            parties[partyID].status = PARTY_READY;
                                        }
                                        Console.WriteLine(DateTime.Now.ToString() + ": " + whisperSender + " added to Group " + partyID);
                                        string temp = "Updated Member List: ";
                                        foreach (var member in parties[partyID].members)
                                        {
                                            temp += member.name + " ";
                                        }
                                        Console.WriteLine(DateTime.Now.ToString() + ": " + temp);
                                    }
                                }
                            }
                            else if (whisperMessage == "!unready")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    CharClass myClass = wolfcoins.classList[whisperSender];
                                    if (myClass.isPartyLeader)
                                    {
                                        if (parties[myClass.groupID].status == PARTY_READY && parties[myClass.groupID].members.Count <= DUNGEON_MAX)
                                        {
                                            parties[myClass.groupID].status = PARTY_FORMING;
                                            Whisper(parties[myClass.groupID], "Party 'Ready' status has been revoked.", group);
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!ready")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    CharClass myClass = wolfcoins.classList[whisperSender];
                                    if (myClass.isPartyLeader)
                                    {
                                        if (parties.ContainsKey(myClass.groupID) && parties[myClass.groupID].status == PARTY_FORMING)
                                        {
                                            parties[myClass.groupID].status = PARTY_READY;
                                            Whisper(parties[myClass.groupID], "Party set to 'Ready'. Be careful adventuring without a full party!", group);
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "n")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].pendingPetRelease)
                                    {
                                        string petName = wolfcoins.classList[whisperSender].toRelease.name;
                                        wolfcoins.classList[whisperSender].pendingPetRelease = false;
                                        wolfcoins.classList[whisperSender].toRelease = new Pet();

                                        Whisper(whisperSender, "You decided to keep " + petName + ".", group);
                                    }

                                    if (wolfcoins.classList[whisperSender].pendingInvite)
                                    {
                                        wolfcoins.classList[whisperSender].pendingInvite = false;
                                        string partyLeader = parties[wolfcoins.classList[whisperSender].groupID].partyLeader;
                                        parties[wolfcoins.classList[whisperSender].groupID].RemoveMember(whisperSender);
                                        wolfcoins.classList[whisperSender].groupID = -1;
                                        Whisper(whisperSender, "You declined " + partyLeader + "'s invite.", group);
                                        Whisper(partyLeader, whisperSender + " has declined your party invite.", group);
                                        wolfcoins.classList[partyLeader].numInvitesSent--;
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!kick"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && wolfcoins.classList[whisperSender].isPartyLeader)
                                {
                                    if (wolfcoins.classList[whisperSender].groupID != -1)
                                    {
                                        if (parties[wolfcoins.classList[whisperSender].groupID].status == PARTY_STARTED)
                                        {
                                            Whisper(whisperSender, "You can't kick a party member in the middle of a dungoen!", group);
                                            continue;
                                        }
                                        if (whispers[1] != null)
                                        {
                                            string[] msgData = whispers[1].Split(' ');

                                            if (msgData.Count() > 1)
                                            {
                                                string toKick = msgData[1];
                                                if (whisperSender == toKick)
                                                {
                                                    Whisper(whisperSender, "You can't kick yourself from a group! Do !leaveparty instead.", group);
                                                    continue;
                                                }
                                                toKick = toKick.ToLower();
                                                if (wolfcoins.classList.Keys.Contains(toKick.ToLower()))
                                                {
                                                    if (wolfcoins.classList[whisperSender].isPartyLeader)
                                                    {
                                                        int myID = wolfcoins.classList[whisperSender].groupID;
                                                        for (int i = 0; i < parties[myID].members.Count(); i++)
                                                        {
                                                            if (parties[myID].members.ElementAt(i).name == toKick)
                                                            {
                                                                parties[myID].RemoveMember(toKick);
                                                                wolfcoins.classList[toKick].groupID = -1;
                                                                wolfcoins.classList[toKick].pendingInvite = false;
                                                                wolfcoins.classList[toKick].numInvitesSent = 0;
                                                                wolfcoins.classList[whisperSender].numInvitesSent--;
                                                                Whisper(toKick, "You were removed from " + whisperSender + "'s party.", group);
                                                                Whisper(parties[myID], toKick + " was removed from the party.", group);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Whisper(whisperSender, "You are not the party leader.", group);
                                                    }
                                                }
                                                else
                                                {
                                                    Whisper(whisperSender, "Couldn't find that party member to remove.", group);
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                            else if (whisperMessage.StartsWith("!add"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && wolfcoins.classList[whisperSender].isPartyLeader)
                                {
                                    if (wolfcoins.classList[whisperSender].groupID != -1)
                                    {
                                        if (parties[wolfcoins.classList[whisperSender].groupID].status == PARTY_STARTED)
                                            continue;

                                        if (wolfcoins.classList[whisperSender].usedGroupFinder && parties[wolfcoins.classList[whisperSender].groupID].NumMembers() == 3)
                                        {
                                            Whisper(whisperSender, "You can't have more than 3 party members for a Group Finder dungeon.", group);
                                            continue;
                                        }

                                        if (whispers[1] != null)
                                        {
                                            string[] msgData = whispers[1].Split(' ');

                                            if (msgData.Count() > 1)
                                            {
                                                string invitee = msgData[1];
                                                if (whisperSender == invitee)
                                                {
                                                    Whisper(whisperSender, "You can't invite yourself to a group!", group);
                                                    continue;
                                                }
                                                if (wolfcoins.Exists(wolfcoins.classList, invitee) && wolfcoins.classList[invitee].queueDungeons.Count > 0)
                                                {
                                                    Whisper(whisperSender, invitee + " is currently queued for Group Finder and cannot be added to the group.", group);
                                                    Whisper(invitee, whisperSender + " tried to invite you to a group, but you are queued in the Group Finder. Type '!leavequeue' to leave the queue.", group);
                                                    continue;
                                                }

                                                invitee = invitee.ToLower();
                                                if (wolfcoins.classList.Keys.Contains(invitee.ToLower()))
                                                {
                                                    int myID = wolfcoins.classList[whisperSender].groupID;
                                                    if (wolfcoins.classList[invitee].classType != -1 && wolfcoins.classList[invitee].groupID == -1
                                                        && !wolfcoins.classList[invitee].pendingInvite && wolfcoins.classList[whisperSender].numInvitesSent < DUNGEON_MAX
                                                        && parties.ContainsKey(myID))
                                                    {
                                                        if (parties[myID].status != PARTY_FORMING && parties[myID].status != PARTY_READY)
                                                        {
                                                            continue;
                                                        }

                                                        wolfcoins.classList[whisperSender].numInvitesSent++;
                                                        wolfcoins.classList[invitee].pendingInvite = true;
                                                        wolfcoins.classList[invitee].groupID = myID;
                                                        wolfcoins.classList[invitee].ClearQueue();
                                                        string myClass = wolfcoins.classList[whisperSender].className;
                                                        int myLevel = wolfcoins.classList[whisperSender].level;
                                                        parties[myID].AddMember(wolfcoins.classList[invitee]);
                                                        string msg = whisperSender + ", Level " + myLevel + " " + myClass + ", has invited you to join a party. Accept? (y/n)";
                                                        Whisper(whisperSender, "You invited " + invitee + " to a group.", group);
                                                        Whisper(invitee, msg, group);
                                                    }
                                                    else if (wolfcoins.classList[whisperSender].numInvitesSent >= DUNGEON_MAX)
                                                    {
                                                        Whisper(whisperSender, "You have the max number of invites already pending.", group);
                                                    }
                                                    else if (wolfcoins.classList[invitee].groupID != -1)
                                                    {
                                                        Whisper(whisperSender, invitee + " is already in a group.", group);
                                                        Whisper(invitee, whisperSender + " tried to invite you to a group, but you are already in one! Type '!leaveparty' to abandon your current group.", group);
                                                    }
                                                }
                                                else
                                                {
                                                    if (wolfcoins.Exists(wolfcoins.xpList, invitee))
                                                    {
                                                        int level = wolfcoins.determineLevel(invitee);
                                                        if (level < 3)
                                                        {
                                                            //Whisper(whisperSender, invitee + " is not high enough level. (" + level + ")", group);
                                                        }
                                                        else
                                                        {
                                                            Whisper(whisperSender, invitee + " is high enough level, but has not picked a class!", group);
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!promote"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    int partyID = wolfcoins.classList[whisperSender].groupID;
                                    if (partyID == -1)
                                    {
                                        continue;
                                    }

                                    if (!wolfcoins.classList[whisperSender].isPartyLeader)
                                    {
                                        Whisper(whisperSender, "You must be the party leader to promote.", group);
                                        continue;
                                    }

                                    if (whispers[1] != null)
                                    {
                                        string[] msgData = whispers[1].Split(' ');

                                        if (msgData.Count() > 1 && msgData.Count() <= 3)
                                        {
                                            string newLeader = msgData[1].ToLower();
                                            bool newLeaderCreated = false;

                                            foreach (var member in parties[partyID].members)
                                            {
                                                if (newLeaderCreated)
                                                    continue;

                                                if (member.name == whisperSender)
                                                    member.isPartyLeader = false;

                                                if (member.name == newLeader)
                                                {

                                                    wolfcoins.classList[whisperSender].isPartyLeader = false;
                                                    parties[partyID].partyLeader = newLeader;
                                                    wolfcoins.classList[newLeader].isPartyLeader = true;
                                                    member.isPartyLeader = true;

                                                    newLeaderCreated = true;
                                                }
                                            }

                                            if (newLeaderCreated)
                                            {
                                                foreach (var member in parties[partyID].members)
                                                {
                                                    if (member.name != newLeader && member.name != whisperSender)
                                                    {
                                                        Whisper(member.name, whisperSender + " has promoted " + newLeader + " to Party Leader.", group);
                                                    }
                                                }
                                                Whisper(newLeader, whisperSender + " has promoted you to Party Leader.", group);
                                                Whisper(whisperSender, "You have promoted " + newLeader + " to Party Leader.", group);
                                            }
                                            else
                                            {
                                                Whisper(whisperSender, "Party member '" + newLeader + "' not found. You are still party leader.", group);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!leaveparty")
                            {
                                if (parties.Count() > 0 && wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    int myID = wolfcoins.classList[whisperSender].groupID;
                                    if (myID != -1 && parties[myID].status == PARTY_STARTED)
                                    {
                                        Whisper(whisperSender, "You can't leave your party while a dungeon is in progress!", group);
                                        continue;
                                    }
                                    if (myID != -1 && !wolfcoins.classList[whisperSender].pendingInvite)
                                    {
                                        if (parties.Count() > 0 && parties.ContainsKey(myID))
                                        {
                                            if (wolfcoins.classList[whisperSender].isPartyLeader)
                                            {
                                                wolfcoins.classList[whisperSender].groupID = -1;
                                                wolfcoins.classList[whisperSender].numInvitesSent = 0;
                                                wolfcoins.classList[whisperSender].isPartyLeader = false;
                                                wolfcoins.classList[whisperSender].ClearQueue();

                                                parties[myID].RemoveMember(whisperSender);
                                                Console.WriteLine("Party Leader " + whisperSender + " left group #" + myID);
                                                string myMembers = "";
                                                foreach (var member in parties[myID].members)
                                                {
                                                    myMembers += member.name + " ";
                                                }
                                                Console.WriteLine(DateTime.Now.ToString() + ": Remaining members: " + myMembers);
                                                Whisper(parties[myID], "The party leader (" + whisperSender + ") has left. Your party has been disbanded.", group);
                                                for (int i = 0; i < parties[myID].members.Count(); i++)
                                                {
                                                    string dude = parties[myID].members.ElementAt(i).name;
                                                    wolfcoins.classList[dude].groupID = -1;
                                                    wolfcoins.classList[dude].pendingInvite = false;
                                                    wolfcoins.classList[dude].numInvitesSent = 0;
                                                    wolfcoins.classList[dude].ClearQueue();

                                                }
                                                parties.Remove(myID);
                                                Whisper(whisperSender, "Your party has been disbanded.", group);

                                            }
                                            else if (parties.ContainsKey(myID) && (parties[myID].RemoveMember(whisperSender)))
                                            {
                                                if (parties[myID].status == PARTY_FORMING)
                                                {
                                                    string partyleader = parties[myID].partyLeader;
                                                    wolfcoins.classList[partyleader].numInvitesSent--;
                                                }
                                                else if (parties[myID].status == PARTY_FULL)
                                                {
                                                    string partyleader = parties[myID].partyLeader;
                                                    parties[myID].status = PARTY_FORMING;
                                                    wolfcoins.classList[partyleader].numInvitesSent--;
                                                }

                                                Whisper(parties[myID], whisperSender + " has left the party.", group);
                                                Whisper(whisperSender, "You left the party.", group);
                                                wolfcoins.classList[whisperSender].groupID = -1;
                                                wolfcoins.classList[whisperSender].ClearQueue();
                                                Console.WriteLine(DateTime.Now.ToString() + ": " + whisperSender + " left group with ID " + myID);
                                                string myMembers = "";
                                                foreach (var member in parties[myID].members)
                                                {
                                                    myMembers += member.name + " ";
                                                }
                                                Console.WriteLine(DateTime.Now.ToString() + ": Remaining Members: " + myMembers);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage == "!daily")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    double minutes = (DateTime.Now - wolfcoins.classList[whisperSender].lastDailyGroupFinder).TotalMinutes;
                                    double totalHours = (DateTime.Now - wolfcoins.classList[whisperSender].lastDailyGroupFinder).TotalHours;
                                    double totalDays = (DateTime.Now - wolfcoins.classList[whisperSender].lastDailyGroupFinder).TotalDays;
                                    if (totalDays >= 1)
                                    {
                                        Whisper(whisperSender, "You are eligible for daily Group Finder rewards! Go queue up!", group);
                                        continue;
                                    }
                                    else
                                    {
                                        double minutesLeft = Math.Truncate(60 - (minutes % 60));
                                        double hoursLeft = Math.Truncate(24 - (totalHours));
                                        Whisper(whisperSender, "Your daily Group Finder reward resets in " + hoursLeft + " hours and " + minutesLeft + " minutes.", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!queue"))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && wolfcoins.classList[whisperSender].name != "NAMELESS ONE")
                                {
                                    if (whisperMessage == "!queuetime")
                                    {
                                        if (wolfcoins.classList[whisperSender].queueDungeons.Count == 0)
                                            continue;

                                        string myQueuedDungeons = "You are queued for the following dungeons: ";
                                        bool firstAdded = false;
                                        foreach (var dung in wolfcoins.classList[whisperSender].queueDungeons)
                                        {
                                            if (!firstAdded)
                                            {
                                                firstAdded = true;
                                            }
                                            else
                                            {
                                                myQueuedDungeons += ",";
                                            }
                                            myQueuedDungeons += dung;
                                        }

                                        double timeSpan = ((DateTime.Now - wolfcoins.classList[whisperSender].queueTime)).TotalSeconds;
                                        double seconds = timeSpan % 60;
                                        seconds = Math.Truncate(seconds);
                                        double minutes = ((DateTime.Now - wolfcoins.classList[whisperSender].queueTime)).TotalMinutes % 60;
                                        minutes = Math.Truncate(minutes);
                                        if (minutes >= 60)
                                            minutes = minutes % 60;

                                        double hours = ((DateTime.Now - wolfcoins.classList[whisperSender].queueTime)).TotalHours;
                                        string timeMessage = "You've been waiting in the Group Finder queue for ";
                                        string lastFormed = "The last group was formed ";
                                        hours = Math.Truncate(hours);
                                        if (hours > 0)
                                            timeMessage += hours + " hours, ";

                                        if (minutes > 0)
                                            timeMessage += minutes + " minutes, and ";

                                        timeMessage += seconds + " seconds.";

                                        timeSpan = ((DateTime.Now - groupFinder.lastFormed).TotalSeconds);
                                        seconds = timeSpan % 60;
                                        seconds = Math.Truncate(seconds);
                                        minutes = timeSpan / 60;
                                        minutes = Math.Truncate(minutes);
                                        hours = minutes / 60;
                                        hours = Math.Truncate(hours);

                                        if (minutes >= 60)
                                            minutes = minutes % 60;

                                        if (hours > 0)
                                            lastFormed += hours + " hours, ";

                                        if (minutes > 0)
                                            lastFormed += minutes + " minutes, and ";

                                        lastFormed += seconds + " seconds ago.";

                                        Whisper(whisperSender, myQueuedDungeons, group);
                                        Whisper(whisperSender, timeMessage, group);
                                        Whisper(whisperSender, lastFormed, group);
                                        continue;
                                    }
                                    if (whisperMessage == "!queuestatus" && (whisperSender == tokenData.BroadcastUser || whisperSender == tokenData.ChatUser))
                                    {
                                        if (groupFinder.queue.Count == 0)
                                        {
                                            Whisper(whisperSender, "No players in queue.", group);
                                        }

                                        Whisper(whisperSender, groupFinder.queue.Count + " players in queue.", group);
                                        Dictionary<int, int> queueData = new Dictionary<int, int>();
                                        foreach (var player in groupFinder.queue)
                                        {
                                            foreach (var dungeonID in player.queueDungeons)
                                            {
                                                if (!queueData.ContainsKey(dungeonID))
                                                {
                                                    queueData.Add(dungeonID, 1);
                                                }
                                                else
                                                {
                                                    queueData[dungeonID]++;
                                                }
                                            }
                                        }

                                        foreach (var dataPoint in queueData)
                                        {
                                            Whisper(whisperSender, "Dungeon ID <" + dataPoint.Key + ">: " + dataPoint.Value + " players", group);
                                        }

                                        continue;
                                    }

                                    if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                    {
                                        Whisper(whisperSender, "You are already queued in the Group Finder! Type !queuetime for more information.", group);
                                        continue;
                                    }

                                    if (!wolfcoins.classList[whisperSender].pendingInvite && wolfcoins.classList[whisperSender].groupID == -1)
                                    {

                                        string[] msgData = whispers[1].Split(' ');
                                        string[] tempDungeonData;
                                        bool didRequest = false;
                                        if (msgData.Count() > 1)
                                        {
                                            tempDungeonData = msgData[1].Split(',');
                                            didRequest = true;
                                        }
                                        else
                                        {
                                            tempDungeonData = GetEligibleDungeons(whisperSender, wolfcoins, dungeonList).Split(',');
                                        }
                                        List<int> requestedDungeons = new List<int>();
                                        //string[] tempDungeonData = msgData[1].Split(',');
                                        string errorMessage = "Unable to join queue. Reason(s): ";
                                        bool eligible = true;
                                        for (int i = 0; i < tempDungeonData.Count(); i++)
                                        {
                                            int tempInt = -1;
                                            int.TryParse(tempDungeonData[i], out tempInt);
                                            int eligibility = DetermineEligibility(whisperSender, tempInt, dungeonList, baseDungeonCost, wolfcoins);
                                            switch (eligibility)
                                            {
                                                case 0: // player not high enough level
                                                    {
                                                        eligible = false;
                                                        errorMessage += "Not appropriate level. (ID: " + tempInt + ") ";
                                                    }
                                                    break;

                                                case -1: // invalid dungeon id
                                                    {
                                                        eligible = false;
                                                        errorMessage += "Invalid Dungeon ID provided. (ID: " + tempInt + ") ";
                                                    }
                                                    break;

                                                case -2: // not enough money
                                                    {
                                                        eligible = false;
                                                        errorMessage += "You don't have enough money!";
                                                    }
                                                    break;
                                                case 1:
                                                    {
                                                        requestedDungeons.Add(tempInt);
                                                    }
                                                    break;

                                                default: break;
                                            }

                                            if (eligibility == -2)
                                                break;

                                            //if (tempInt != -1)
                                            //    requestedDungeons.Add(tempInt);

                                        }

                                        if (!eligible)
                                        {
                                            Whisper(whisperSender, errorMessage, group);
                                            continue;
                                        }

                                        wolfcoins.classList[whisperSender].queuePriority = groupFinder.priority;
                                        groupFinder.priority++;
                                        wolfcoins.classList[whisperSender].usedGroupFinder = true;
                                        wolfcoins.classList[whisperSender].queueDungeons = requestedDungeons;

                                        Party myParty = groupFinder.Add(wolfcoins.classList[whisperSender]);
                                        if (myParty.members.Count != 3)
                                        {
                                            wolfcoins.classList[whisperSender].queueTime = DateTime.Now;
                                            Whisper(whisperSender, "You have been placed in the Group Finder queue.", group);
                                            continue;
                                        }

                                        myParty.members.ElementAt(0).isPartyLeader = true;
                                        myParty.partyLeader = myParty.members.ElementAt(0).name;
                                        myParty.status = PARTY_FULL;
                                        myParty.members.ElementAt(0).numInvitesSent = 3;
                                        myParty.myID = maxPartyID;
                                        myParty.usedDungeonFinder = true;

                                        int lowestNumOfDungeons = myParty.members.ElementAt(0).queueDungeons.Count;
                                        int pickiestMember = 0;
                                        int count = 0;

                                        // Pick the party member with the least available dungeons (to narrow down options)
                                        foreach (var member in myParty.members)
                                        {

                                            if (member.name == myParty.partyLeader)
                                                continue;

                                            count++;

                                            if (member.queueDungeons.Count < lowestNumOfDungeons)
                                            {
                                                pickiestMember = count;
                                                lowestNumOfDungeons = member.queueDungeons.Count;
                                            }

                                        }

                                        Random RNG = new Random();
                                        int numAvailableDungeons = myParty.members.ElementAt(pickiestMember).queueDungeons.Count();

                                        //choose a random dungeon out of the available options
                                        int randDungeon = RNG.Next(0, (numAvailableDungeons - 1));
                                        // set the id based on that random dungeon
                                        int dungeonID = (myParty.members.ElementAt(pickiestMember).queueDungeons.ElementAt(randDungeon)) - 1;
                                        dungeonID++;
                                        myParty.members.ElementAt(0).groupFinderDungeon = dungeonID;
                                        string dungeonName = GetDungeonName(dungeonID, dungeonList);
                                        string members = "Group Finder group created for " + dungeonName + ": ";
                                        foreach (var member in myParty.members)
                                        {
                                            member.groupID = maxPartyID;
                                            member.usedGroupFinder = true;
                                            members += member.name + ", " + member.className + "; ";
                                            string otherMembers = "";
                                            foreach (var player in myParty.members)
                                            {
                                                if (player.name == member.name)
                                                    continue;

                                                otherMembers += player.name + " (" + player.className + ") ";
                                            }

                                            Whisper(member.name, "You've been matched for " + dungeonName + " with: " + otherMembers + ".", group);
                                            if (member.isPartyLeader)
                                                Whisper(member.name, "You are the party leader. Whisper me '!start' to begin!", group);
                                        }
                                        parties.Add(maxPartyID, myParty);
                                        Console.WriteLine(DateTime.Now.ToString() + ": " + members);
                                        maxPartyID++;

                                    }
                                    else if (wolfcoins.classList[whisperSender].isPartyLeader)
                                    {
                                        string reason = "";
                                        if (parties.ContainsKey(wolfcoins.classList[whisperSender].groupID))
                                        {
                                            switch (parties[wolfcoins.classList[whisperSender].groupID].status)
                                            {
                                                case PARTY_FORMING:
                                                    {
                                                        reason = "Party is currently forming. Add members with '!add <username>'";
                                                    }
                                                    break;

                                                case PARTY_READY:
                                                    {
                                                        reason = "Party is filled and ready to adventure! Type '!start' to begin!";
                                                    }
                                                    break;

                                                case PARTY_STARTED:
                                                    {
                                                        reason = "Your party is currently on an adventure!";
                                                    }
                                                    break;

                                                case PARTY_COMPLETE:
                                                    {
                                                        reason = "Your party just finished an adventure!";
                                                    }
                                                    break;

                                                default:
                                                    {
                                                        reason = "I have no idea the status of your party.";
                                                    }
                                                    break;
                                            }
                                        }
                                        Whisper(whisperSender, "You already have a party created! " + reason, group);
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You currently have an outstanding invite to another party. Couldn't create new party!", group);
                                    }
                                }
                            }
                            else if (whisperMessage == "!classes")
                            {
                                if (wolfcoins.classList != null)
                                {
                                    double numClasses = 0;
                                    double numWarriors = 0;
                                    double numMages = 0;
                                    double numRogues = 0;
                                    double numRangers = 0;
                                    double numClerics = 0;
                                    foreach (var member in wolfcoins.classList)
                                    {
                                        numClasses++;
                                        switch (member.Value.classType)
                                        {
                                            case CharClass.WARRIOR:
                                                {
                                                    numWarriors++;
                                                }
                                                break;

                                            case CharClass.MAGE:
                                                {
                                                    numMages++;
                                                }
                                                break;

                                            case CharClass.ROGUE:
                                                {
                                                    numRogues++;
                                                }
                                                break;

                                            case CharClass.RANGER:
                                                {
                                                    numRangers++;
                                                }
                                                break;

                                            case CharClass.CLERIC:
                                                {
                                                    numClerics++;
                                                }
                                                break;

                                            default: break;
                                        }


                                    }

                                    double percentWarriors = (numWarriors / numClasses) * 100;
                                    percentWarriors = Math.Round(percentWarriors, 1);
                                    double percentMages = (numMages / numClasses) * 100;
                                    percentMages = Math.Round(percentMages, 1);
                                    double percentRogues = (numRogues / numClasses) * 100;
                                    percentRogues = Math.Round(percentRogues, 1);
                                    double percentRangers = (numRangers / numClasses) * 100;
                                    percentRangers = Math.Round(percentRangers, 1);
                                    double percentClerics = (numClerics / numClasses) * 100;
                                    percentClerics = Math.Round(percentClerics, 1);

                                    Whisper(whisperSender, "Class distribution for the Wolfpack RPG: ", group);
                                    Whisper(whisperSender, "Warriors: " + percentWarriors + "%", group);
                                    Whisper(whisperSender, "Mages: " + percentMages + "%", group);
                                    Whisper(whisperSender, "Rogues: " + percentRogues + "%", group);
                                    Whisper(whisperSender, "Rangers: " + percentRangers + "%", group);
                                    Whisper(whisperSender, "Clerics " + percentClerics + "%", group);
                                }
                            }
                            else if (whisperMessage == "!leavequeue")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                    {
                                        groupFinder.RemoveMember(whisperSender);
                                        wolfcoins.classList[whisperSender].ClearQueue();

                                        Whisper(whisperSender, "You were removed from the Group Finder.", group);
                                    }
                                }
                            }
                            else if (whisperMessage == "!createparty")
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (!wolfcoins.classList[whisperSender].pendingInvite && wolfcoins.classList[whisperSender].groupID == -1)
                                    {
                                        if (wolfcoins.classList[whisperSender].queueDungeons.Count > 0)
                                        {
                                            Whisper(whisperSender, "Can't create a party while queued with the Group Finder. Message me '!leavequeue' to exit.", group);
                                            continue;
                                        }
                                        wolfcoins.classList[whisperSender].isPartyLeader = true;
                                        wolfcoins.classList[whisperSender].numInvitesSent = 1;
                                        Wolfcoins.Party myParty = new Wolfcoins.Party();
                                        myParty.status = PARTY_FORMING;
                                        myParty.partyLeader = whisperSender;
                                        int myLevel = wolfcoins.determineLevel(whisperSender);
                                        wolfcoins.classList[whisperSender].groupID = maxPartyID;
                                        myParty.AddMember(wolfcoins.classList[whisperSender]);
                                        myParty.myID = maxPartyID;
                                        parties.Add(maxPartyID, myParty);

                                        Whisper(whisperSender, "Party created! Use '!add <username>' to invite party members.", group);
                                        Console.WriteLine(DateTime.Now.ToString() + ": Party created: ");
                                        Console.WriteLine(DateTime.Now.ToString() + ": ID: " + maxPartyID);
                                        Console.WriteLine(DateTime.Now.ToString() + ": Total number of parties: " + parties.Count());
                                        maxPartyID++;
                                    }
                                    else if (wolfcoins.classList[whisperSender].isPartyLeader)
                                    {
                                        string reason = "";
                                        if (parties.ContainsKey(wolfcoins.classList[whisperSender].groupID))
                                        {
                                            switch (parties[wolfcoins.classList[whisperSender].groupID].status)
                                            {
                                                case PARTY_FORMING:
                                                    {
                                                        reason = "Party is currently forming. Add members with '!add <username>'";
                                                    }
                                                    break;

                                                case PARTY_READY:
                                                    {
                                                        reason = "Party is filled and ready to adventure! Type '!start' to begin!";
                                                    }
                                                    break;

                                                case PARTY_STARTED:
                                                    {
                                                        reason = "Your party is currently on an adventure!";
                                                    }
                                                    break;

                                                case PARTY_COMPLETE:
                                                    {
                                                        reason = "Your party just finished an adventure!";
                                                    }
                                                    break;

                                                default:
                                                    {
                                                        reason = "I have no idea the status of your party.";
                                                    }
                                                    break;
                                            }
                                        }
                                        Whisper(whisperSender, "You already have a party created! " + reason, group);
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You currently have an outstanding invite to another party. Couldn't create new party!", group);
                                    }
                                }
                            }

                            else if (whisperMessage.Equals("nevermind", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                {
                                    if (wolfcoins.classList[whisperSender].classType >= 10)
                                    {

                                        int oldClass = wolfcoins.classList[whisperSender].classType / 10;
                                        wolfcoins.classList[whisperSender].classType = oldClass;
                                        Whisper(whisperSender, "Respec cancelled. No Wolfcoins deducted from your balance.", group);
                                    }

                                }
                            }

                            else if (whisperMessage == "!partydata")
                            {
                                if (parties.Count() > 0)
                                {
                                    if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                    {
                                        int myID = wolfcoins.classList[whisperSender].groupID;
                                        if (myID != -1)
                                        {
                                            string partyMembers = "";
                                            if (parties.ContainsKey(myID))
                                            {
                                                for (int i = 0; i < parties[myID].members.Count(); i++)
                                                {
                                                    partyMembers += parties[myID].members.ElementAt(i).name + " ";
                                                }
                                                string status = "";
                                                switch (parties[myID].status)
                                                {
                                                    case PARTY_FORMING:
                                                        {
                                                            status = "PARTY_FORMING";
                                                        }
                                                        break;

                                                    case PARTY_READY:
                                                        {
                                                            status = "PARTY_READY";
                                                        }
                                                        break;

                                                    case PARTY_STARTED:
                                                        {
                                                            status = "PARTY_STARTED";
                                                        }
                                                        break;

                                                    case PARTY_COMPLETE:
                                                        {
                                                            status = "PARTY_COMPLETE";
                                                        }
                                                        break;

                                                    default:
                                                        {
                                                            status = "UNKNOWN_STATUS";
                                                        }
                                                        break;
                                                }
                                                irc.sendChatMessage(whisperSender + " requested his Party Data. Group ID: " + wolfcoins.classList[whisperSender].groupID + "; Members: " + partyMembers + "; Status: " + status);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!clearitems"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 1)
                                {
                                    string target = whisperMSG[1];
                                    if (wolfcoins.Exists(wolfcoins.classList, target))
                                    {
                                        wolfcoins.classList[target].totalItemCount = 0;
                                        wolfcoins.classList[target].myItems = new List<Item>();
                                        wolfcoins.SaveClassData();
                                        Whisper(whisperSender, "Cleared " + target + "'s item list.", group);
                                    }
                                }
                            }
                            else if (whisperMessage == "!fixstats")
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                if (wolfcoins.classList != null)
                                {
                                    foreach (var user in wolfcoins.classList)
                                    {
                                        if (user.Value.name == "NAMELESS ONE")
                                            continue;

                                        int classType = wolfcoins.classList[user.Value.name].classType;
                                        CharClass defaultClass;
                                        switch (classType)
                                        {
                                            case CharClass.WARRIOR:
                                                {
                                                    defaultClass = new Warrior();
                                                }
                                                break;

                                            case CharClass.MAGE:
                                                {
                                                    defaultClass = new Mage();
                                                }
                                                break;

                                            case CharClass.ROGUE:
                                                {
                                                    defaultClass = new Rogue();
                                                }
                                                break;

                                            case CharClass.RANGER:
                                                {
                                                    defaultClass = new Ranger();
                                                }
                                                break;

                                            case CharClass.CLERIC:
                                                {
                                                    defaultClass = new Cleric();
                                                }
                                                break;

                                            default:
                                                {
                                                    defaultClass = new CharClass();
                                                }
                                                break;
                                        }

                                        wolfcoins.classList[user.Value.name].coinBonus = defaultClass.coinBonus;
                                        wolfcoins.classList[user.Value.name].xpBonus = defaultClass.xpBonus;
                                        wolfcoins.classList[user.Value.name].itemFind = defaultClass.itemFind;
                                        wolfcoins.classList[user.Value.name].successChance = defaultClass.successChance;
                                        wolfcoins.classList[user.Value.name].preventDeathBonus = defaultClass.preventDeathBonus;
                                        wolfcoins.classList[user.Value.name].itemEarned = -1;
                                        wolfcoins.classList[user.Value.name].ClearQueue();

                                    }
                                    wolfcoins.SaveClassData();
                                    Whisper(whisperSender, "Reset all user's stats to default.", group);
                                }

                            }
                            else if (whisperMessage.StartsWith("!giveitem"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 2)
                                {
                                    string temp = whisperMSG[2];
                                    int id = -1;
                                    int.TryParse(temp, out id);
                                    if (id < 1 || id > itemDatabase.Count())
                                    {
                                        Whisper(whisperSender, "Invalid ID was attempted to be given.", group);
                                    }

                                    string user = whisperMSG[1];
                                    bool hasItem = false;
                                    if (wolfcoins.Exists(wolfcoins.classList, user))
                                    {
                                        if (wolfcoins.classList[user].myItems.Count > 0)
                                        {

                                            foreach (var item in wolfcoins.classList[user].myItems)
                                            {
                                                if (item.itemID == id)
                                                {
                                                    hasItem = true;
                                                    Whisper(whisperSender, user + " already has " + itemDatabase[id - 1].itemName + ".", group);
                                                }
                                            }

                                        }
                                        if (!hasItem && itemDatabase.ContainsKey(id))
                                        {
                                            GrantItem(id, wolfcoins, user, itemDatabase);
                                            wolfcoins.SaveClassData();
                                            //if(wolfcoins.classList[user].totalItemCount != -1)
                                            //{
                                            //    wolfcoins.classList[user].totalItemCount++;
                                            //}
                                            //else
                                            //{
                                            //    wolfcoins.classList[user].totalItemCount = 1;
                                            //}
                                            //wolfcoins.classList[user].myItems.Add(itemDatabase[id]);
                                            Whisper(whisperSender, "Gave " + user + " a " + itemDatabase[id - 1].itemName + ".", group);
                                        }

                                    }
                                }
                            }
                            // player requests to equip an item. make sure they actually *have* items
                            // check if their item is equippable ('other' type items are not), and that it isn't already active
                            // set it to true, then iterate through item list. if inventoryID does *not* match and it *IS* active, deactivate it
                            else if (whisperMessage.StartsWith("!activate") || whisperMessage.StartsWith("activate") || whisperMessage.StartsWith("!equip")
                                || whisperMessage.StartsWith("equip"))
                            {
                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 1)
                                {

                                    string temp = whisperMSG[1];
                                    int id = -1;
                                    int.TryParse(temp, out id);
                                    if (wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                    {
                                        if (wolfcoins.classList[whisperSender].myItems.Count > 0)
                                        {
                                            Item toActivate = wolfcoins.classList[whisperSender].GetItem(id);
                                            int itemPos = wolfcoins.classList[whisperSender].GetItemPos(id);

                                            if (toActivate.itemType == Item.TYPE_ARMOR || toActivate.itemType == Item.TYPE_WEAPON)
                                            {
                                                if (toActivate.isActive)
                                                {
                                                    Whisper(whisperSender, toActivate.itemName + " is already equipped.", group);
                                                    continue;
                                                }
                                                wolfcoins.classList[whisperSender].myItems.ElementAt(itemPos).isActive = true;
                                                foreach (var itm in wolfcoins.classList[whisperSender].myItems)
                                                {
                                                    if (itm.inventoryID == id)
                                                        continue;

                                                    if (itm.itemType != toActivate.itemType)
                                                        continue;

                                                    if (itm.isActive)
                                                    {
                                                        itm.isActive = false;
                                                        Whisper(whisperSender, "Unequipped " + itm.itemName + ".", group);
                                                    }
                                                }
                                                Whisper(whisperSender, "Equipped " + toActivate.itemName + ".", group);
                                            }
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "You have no items.", group);
                                        }
                                    }
                                }
                            }
                            // player requests to unequip an item. make sure they actually *have* items
                            // check if their item is equippable ('other' type items are not), and that it isn't already inactive
                            else if (whisperMessage.StartsWith("!deactivate") || whisperMessage.StartsWith("deactivate") || whisperMessage.StartsWith("!unequip")
                            || whisperMessage.StartsWith("unequip"))
                            {
                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 1)
                                {

                                    string temp = whisperMSG[1];
                                    int id = -1;
                                    int.TryParse(temp, out id);
                                    if (wolfcoins.Exists(wolfcoins.classList, whisperSender) && id != -1)
                                    {
                                        if (wolfcoins.classList[whisperSender].myItems.Count > 0)
                                        {
                                            Item toDeactivate = wolfcoins.classList[whisperSender].GetItem(id);
                                            int itemPos = wolfcoins.classList[whisperSender].GetItemPos(id);

                                            if (toDeactivate.itemType == Item.TYPE_ARMOR || toDeactivate.itemType == Item.TYPE_WEAPON)
                                            {
                                                if (toDeactivate.isActive)
                                                {
                                                    wolfcoins.classList[whisperSender].myItems.ElementAt(itemPos).isActive = false;
                                                    Whisper(whisperSender, "Unequipped " + toDeactivate.itemName + ".", group);
                                                }

                                            }
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "You have no items.", group);
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!printinfo"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    break;
                                // first[1] is the user to print info for
                                if (whispers.Length >= 2 && whispers[1] != null)
                                {
                                    string[] whisperMSG = whispers[1].Split();
                                    string player = whisperMSG[1].ToString();
                                    if (wolfcoins.Exists(wolfcoins.classList, player))
                                    {

                                        // print out all the user's info

                                        int numItems = wolfcoins.classList[player].totalItemCount;

                                        Console.WriteLine("Name: " + player);
                                        Console.WriteLine("Level: " + wolfcoins.classList[player].level);
                                        Console.WriteLine("Prestige Level: " + wolfcoins.classList[player].prestige);
                                        Console.WriteLine("Class: " + wolfcoins.classList[player].className);
                                        Console.WriteLine("Dungeon success chance: " + wolfcoins.classList[player].GetTotalSuccessChance());
                                        Console.WriteLine("Number of Items: " + numItems);
                                        Console.WriteLine(wolfcoins.classList[player].PrintItems());

                                    }
                                    else
                                    {
                                        Console.WriteLine("Player name not found.");
                                    }

                                }
                            }
                            else if (whisperMessage.StartsWith("!setxp"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 2)
                                {
                                    if (int.TryParse(whisperMSG[2], out int value))
                                    {
                                        int newXp = wolfcoins.SetXP(value, whisperMSG[1], group);
                                        if (newXp != -1)
                                        {
                                            Whisper(whisperSender, "Set " + whisperMSG[1] + "'s XP to " + newXp + ".", group);
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "Error updating XP amount.", group);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "Invalid data provided for !setxp command.", group);
                                    }
                                }
                                else
                                {
                                    Whisper(whisperSender, "Not enough data provided for !setxp command.", group);
                                }
                            }
                            else if (whisperMessage.StartsWith("!setprestige"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 2)
                                {
                                    int value = -1;
                                    if (int.TryParse(whisperMSG[2], out value))
                                    {

                                        if (value != -1 && wolfcoins.classList.ContainsKey(whisperMSG[1]))
                                        {
                                            wolfcoins.classList[whisperMSG[1].ToString()].prestige = value;
                                            Whisper(whisperSender, "Set " + whisperMSG[1] + "'s Prestige to " + value + ".", group);
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "Error updating Prestige Level.", group);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "Invalid data provided for !setprestige command.", group);
                                    }
                                }
                                else
                                {
                                    Whisper(whisperSender, "Not enough data provided for !setprestige command.", group);
                                }
                            }
                            else if (whisperMessage.StartsWith("C") || whisperMessage.StartsWith("c"))
                            {
                                if (wolfcoins.classList != null)
                                {
                                    if (wolfcoins.classList.Keys.Contains(whisperSender.ToLower()))
                                    {
                                        if (wolfcoins.classList[whisperSender].classType == -1)
                                        {
                                            wolfcoins.SetClass(whisperSender, whisperMessage, group);
                                        }

                                        if (wolfcoins.classList[whisperSender].classType.ToString().EndsWith("0"))
                                        {
                                            char c = whisperMessage.Last();
                                            int newClass = -1;
                                            int.TryParse(c.ToString(), out newClass);
                                            wolfcoins.ChangeClass(whisperSender, newClass, group);
                                        }

                                    }
                                }
                            }
                            //
                            //
                            //               COMMANDS TO FIX STUFF
                            //
                            //
                            else if (whisperMessage == "!patch1")
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                if (wolfcoins.xpList != null)
                                {
                                    CharClass emptyClass = new CharClass();
                                    emptyClass.classType = -1;
                                    emptyClass.totalItemCount = -1;

                                    foreach (var viewer in wolfcoins.xpList)
                                    {
                                        int myLevel = wolfcoins.determineLevel(viewer.Key);
                                        if (myLevel >= 3 && !wolfcoins.classList.ContainsKey(viewer.Key))
                                        {
                                            emptyClass.name = viewer.Key;
                                            emptyClass.level = myLevel;
                                            wolfcoins.classList.Add(viewer.Key, emptyClass);
                                            Console.WriteLine("Added " + viewer.Key + " to the Class List.");
                                        }
                                    }
                                    wolfcoins.SaveClassData();
                                }
                            }
                            // command to fix multiple inventory ids and active states
                            else if (whisperMessage == "!fixinventory")
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    continue;

                                foreach (var player in wolfcoins.classList)
                                {
                                    player.Value.FixItems();
                                }

                                //Console.WriteLine(wolfcoins.classList["kraad_"].FixItems());

                                wolfcoins.SaveClassData();
                            }

                            else if (whisperMessage == "1")
                            {
                                Whisper(whisperSender, "Wolfcoins are a currency you earn by watching the stream! You can check your coins by whispering me '!coins' or '!stats'. To find out what you can spend coins on, message me '!shop'.", group);
                            }

                            else if (whisperMessage == "2")
                            {
                                Whisper(whisperSender, "Did you know you gain experience by watching the stream? You can level up as you get more XP! Max level is 20. To check your level & xp, message me '!xp' '!level' or '!stats'. Only Level 2+ viewers can post links. This helps prevent bot spam!", group);
                            }

                            else if (whisperMessage == "!shop")
                            {
                                Whisper(whisperSender, "Whisper me '!stats <username>' to check another users stats! (Cost: 1 coin)   Whisper me '!gloat' to spend 10 coins and show off your level! (Cost: 10 coins)", group);
                            }

                            else if (whisperMessage == "!dungeonlist")
                            {
                                Whisper(whisperSender, "List of Wolfpack RPG Adventures: http://tinyurl.com/WolfpackAdventureList", group);
                            }
                            else if (whisperMessage == "!debugcatch")
                            {
                                if (whisperSender == tokenData.BroadcastUser || whisperSender == tokenData.ChatUser)
                                {
                                    for (int i = 0; i < 50; i++)
                                    {
                                        Fish randomFish = new Fish(WeightedRandomFish(ref fishDatabase));
                                        Console.WriteLine(randomFish.name + " (Rarity " + randomFish.rarity + ") caught.");
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!debuglevel5"))
                            {
                                if (whisperSender == tokenData.BroadcastUser || whisperSender == tokenData.ChatUser)
                                {
                                    string[] whisperMSG = whispers[1].Split();
                                    if (whisperMSG.Length > 1)
                                    {
                                        string user = whisperMSG[1];
                                        if (wolfcoins.Exists(wolfcoins.classList, user))
                                        {
                                            wolfcoins.classList.Remove(user);
                                            wolfcoins.SetXP(1, user, group);
                                            wolfcoins.SetXP(600, user, group);
                                        }
                                        else
                                        {
                                            wolfcoins.SetXP(1, user, group);
                                            wolfcoins.SetXP(600, user, group);
                                        }
                                    }
                                }
                            }

                            else if (whisperMessage.StartsWith("!clearclass"))
                            {
                                if (whisperSender == tokenData.BroadcastUser || whisperSender == tokenData.ChatUser)
                                {
                                    if (wolfcoins.classList != null)
                                    {
                                        string[] whisperMSG = whispers[1].Split();
                                        if (whisperMSG.Length > 1)
                                        {
                                            string user = whisperMSG[1];
                                            if (wolfcoins.classList.Keys.Contains(user.ToLower()))
                                            {
                                                wolfcoins.classList.Remove(user);
                                                wolfcoins.SaveClassData();
                                                Whisper(whisperSender, "Cleared " + user + "'s class.", group);
                                            }
                                            else
                                            {
                                                Whisper(whisperSender, "Couldn't find you in the class table.", group);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!setcoins"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    break;

                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 2)
                                {
                                    if (int.TryParse(whisperMSG[2], out int value))
                                    {
                                        if (!wolfcoins.Exists(wolfcoins.coinList, whisperMSG[1]))
                                        {
                                            wolfcoins.coinList.Add(whisperMSG[1], 0);
                                        }
                                        int newCoins = wolfcoins.SetCoins(value, whisperMSG[1]);
                                        if (newCoins != -1)
                                        {
                                            Whisper(whisperSender, "Set " + whisperMSG[1] + "'s coins to " + newCoins + ".", group);
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "Error updating Coin amount.", group);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "Invalid data provided for !setcoins command.", group);
                                    }
                                }
                                else
                                {
                                    Whisper(whisperSender, "Not enough data provided for !setcoins command.", group);
                                }
                            }

                            if (whisperMessage == "!coins" || whisperMessage == "coins")
                            {
                                if (wolfcoins.coinList != null)
                                {
                                    if (wolfcoins.coinList.ContainsKey(whisperSender))
                                    {

                                        Whisper(whisperSender, "You have: " + wolfcoins.coinList[whisperSender] + " coins.", group);
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have any coins yet! Stick around during the livestream to earn coins.", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!gloatpet") || whisperMessage.StartsWith("!petgloat"))
                            {
                                if (wolfcoins.classList.ContainsKey(whisperSender))
                                {
                                    if (wolfcoins.coinList[whisperSender] < gloatCost)
                                    {
                                        Whisper(whisperSender, "You don't have enough coins to gloat!", group);
                                        continue;
                                    }

                                    if (wolfcoins.classList[whisperSender].myPets.Count > 0)
                                    {
                                        bool hasActive = false;
                                        Pet toGloat = new Pet();
                                        foreach (var pet in wolfcoins.classList[whisperSender].myPets)
                                        {
                                            if (pet.isActive)
                                            {
                                                hasActive = true;
                                                toGloat = pet;
                                                break;
                                            }
                                        }

                                        if (!hasActive)
                                        {
                                            Whisper(whisperSender, "You don't have an active pet to show off! Activate one with !summon <id>", group);
                                            continue;
                                        }

                                        string temp = gloatCost.ToString();
                                        wolfcoins.RemoveCoins(whisperSender, temp);


                                        string petType = "";
                                        if (toGloat.isSparkly)
                                            petType += "SPARKLY " + toGloat.type;
                                        else
                                            petType = toGloat.type;

                                        irc.sendChatMessage(whisperSender + " watches proudly as their level " + toGloat.level + " " + petType + " named " + toGloat.name + " struts around!");
                                        Whisper(whisperSender, "You spent " + temp + " wolfcoins to brag about " + toGloat.name + ".", group);
                                    }
                                }

                            }
                            else if (whisperMessage.StartsWith("!gloatfish") || whisperMessage.StartsWith("!fishgloat"))
                            {
                                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                                {
                                    if (wolfcoins.coinList[whisperSender] < gloatCost)
                                    {
                                        Whisper(whisperSender, "You don't have enough coins to gloat!", group);
                                        continue;
                                    }

                                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                                    {
                                        Fish toGloat = new Fish();
                                        string[] msgData = whispers[1].Split(' ');
                                        if (msgData.Count() != 2)
                                        {
                                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !fish <Fish #>", group);
                                            continue;
                                        }
                                        int fishID = -1;
                                        if (int.TryParse(msgData[1], out fishID))
                                        {
                                            if (fishID <= wolfcoins.fishingList[whisperSender].biggestFish.Count && fishID > 0)
                                            {
                                                string temp = gloatCost.ToString();
                                                wolfcoins.RemoveCoins(whisperSender, temp);

                                                Fish tempFish = new Fish(wolfcoins.fishingList[whisperSender].biggestFish.ElementAt(fishID - 1));

                                                irc.sendChatMessage(whisperSender + " gloats about the time they caught a  " + tempFish.length + " in. long, " + tempFish.weight + " pound " + tempFish.name + " lobosSmug");
                                                Whisper(whisperSender, "You spent " + temp + " wolfcoins to brag about your biggest" + tempFish.name + ".", group);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have any fish! Type !cast to try and fish for some!", group);
                                    }


                                }
                            }
                            else if (whisperMessage.StartsWith("!gloat") || whisperMessage.StartsWith("gloat"))
                            {
                                if (wolfcoins.coinList != null && wolfcoins.xpList != null)
                                {
                                    if (wolfcoins.coinList.ContainsKey(whisperSender) && wolfcoins.xpList.ContainsKey(whisperSender))
                                    {

                                        if (wolfcoins.coinList[whisperSender] >= gloatCost)
                                        {
                                            string temp = gloatCost.ToString();
                                            string gloatMessage = "";
                                            int level = wolfcoins.determineLevel(whisperSender);
                                            string levelWithPrestige = wolfcoins.gloatWithPrestige(whisperSender);
                                            wolfcoins.RemoveCoins(whisperSender, temp);
                                            #region gloatMessages
                                            switch (level)
                                            {
                                                case 1:
                                                    {
                                                        gloatMessage = "Just a baby! lobosMindBlank";
                                                    }
                                                    break;

                                                case 2:
                                                    {
                                                        gloatMessage = "Scrubtastic!";
                                                    }
                                                    break;

                                                case 3:
                                                    {
                                                        gloatMessage = "Pretty weak!";
                                                    }
                                                    break;

                                                case 4:
                                                    {
                                                        gloatMessage = "Not too shabby.";
                                                    }
                                                    break;

                                                case 5:
                                                    {
                                                        gloatMessage = "They can hold their own!";
                                                    }
                                                    break;

                                                case 6:
                                                    {
                                                        gloatMessage = "Getting pretty strong Kreygasm";
                                                    }
                                                    break;

                                                case 7:
                                                    {
                                                        gloatMessage = "A formidable opponent!";
                                                    }
                                                    break;

                                                case 8:
                                                    {
                                                        gloatMessage = "A worthy adversary!";
                                                    }
                                                    break;

                                                case 9:
                                                    {
                                                        gloatMessage = "A most powerful combatant!";
                                                    }
                                                    break;

                                                case 10:
                                                    {
                                                        gloatMessage = "A seasoned war veteran!";
                                                    }
                                                    break;

                                                case 11:
                                                    {
                                                        gloatMessage = "A fearsome champion of the Wolfpack!";
                                                    }
                                                    break;

                                                case 12:
                                                    {
                                                        gloatMessage = "A vicious pack leader!";
                                                    }
                                                    break;

                                                case 13:
                                                    {
                                                        gloatMessage = "A famed Wolfpack Captain!";
                                                    }
                                                    break;

                                                case 14:
                                                    {
                                                        gloatMessage = "A brutal commander of the Wolfpack!";
                                                    }
                                                    break;

                                                case 15:
                                                    {
                                                        gloatMessage = "Decorated Chieftain of the Wolfpack!";
                                                    }
                                                    break;

                                                case 16:
                                                    {
                                                        gloatMessage = "A War Chieftain of the Wolfpack!";
                                                    }
                                                    break;

                                                case 17:
                                                    {
                                                        gloatMessage = "A sacred Wolfpack Justicar!";
                                                    }
                                                    break;

                                                case 18:
                                                    {
                                                        gloatMessage = "Demigod of the Wolfpack!";
                                                    }
                                                    break;

                                                case 19:
                                                    {
                                                        gloatMessage = "A legendary Wolfpack demigod veteran!";
                                                    }
                                                    break;

                                                case 20:
                                                    {
                                                        gloatMessage = "The Ultimate Wolfpack God Rank. A truly dedicated individual.";
                                                    }
                                                    break;

                                                default: break;
                                            }
                                            #endregion

                                            irc.sendChatMessage(whisperSender + " has spent " + gloatCost + " Wolfcoins to show off that they are " + levelWithPrestige + "! " + gloatMessage);
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "You don't have enough coins to gloat (Cost: " + gloatCost + " Wolfcoins)", group);
                                        }
                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have coins and/or xp yet!", group);
                                    }
                                }
                            }
                            else if ((whisperMessage.StartsWith("!bet") || whisperMessage.StartsWith("bet")) && betsAllowed && betActive && wolfcoins.Exists(wolfcoins.coinList, whisperSender))
                            {
                                string[] whisperMSG = whispers[1].Split();
                                if (whisperMSG.Length > 1)
                                {
                                    Better betInfo = new Better();
                                    string user = whispers[1];
                                    string vote = whispers[2].ToLower();
                                    int betAmount = -1;
                                    if (int.TryParse(whispers[3], out betAmount))
                                    {
                                        if (!wolfcoins.CheckCoins(user, betAmount))
                                        {
                                            Whisper(user, "There was an error placing your bet. (not enough coins?)", group);
                                            continue;
                                        }
                                        betInfo.betAmount = betAmount;
                                        if (!betters.ContainsKey(user))
                                        {
                                            wolfcoins.RemoveCoins(user, betAmount.ToString());
                                            if (vote == "succeed")
                                            {
                                                betInfo.vote = SUCCEED;
                                                betters.Add(user, betInfo);

                                            }
                                            else if (vote == "fail")
                                            {
                                                betInfo.vote = FAIL;
                                                betters.Add(user, betInfo);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!givexp"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    break;

                                string[] whisperMSG = whispers[1].Split();

                                if (whisperMSG[0] != null && whisperMSG[1] != null && whisperMSG[2] != null)
                                {
                                    if (!(int.TryParse(whisperMSG[2].ToString(), out int value)))
                                    {
                                        break;
                                    }
                                    string user = whisperMSG[1].ToString();

                                    wolfcoins.AwardXP(value, user, group);
                                    wolfcoins.SaveClassData();
                                    wolfcoins.SaveXP();
                                    Whisper(whisperSender, "Gave " + user + " " + value + " XP.", group);
                                }
                                else
                                {
                                    irc.sendChatMessage("Not enough data provided for !givexp command.");
                                }
                            }
                            else if (whisperMessage.StartsWith("!xp") || whisperMessage.StartsWith("xp") || whisperMessage.StartsWith("level") || whisperMessage.StartsWith("!level") ||
                                whisperMessage.StartsWith("!lvl") || whisperMessage.StartsWith("lvl"))
                            {
                                if (wolfcoins.xpList != null)
                                {
                                    if (wolfcoins.xpList.ContainsKey(whisperSender))
                                    {

                                        int myLevel = wolfcoins.determineLevel(whisperSender);
                                        int xpToNextLevel = wolfcoins.XpToNextLevel(whisperSender);
                                        int myPrestige = -1;
                                        if (wolfcoins.classList.ContainsKey(whisperSender))
                                        {
                                            myPrestige = wolfcoins.classList[whisperSender].prestige;
                                        }
                                        //if(!wolfcoins.Exists(wolfcoins.classList, whisperSender))
                                        //{
                                        //    Whisper(whisperSender, "You are Level " + myLevel + " (Total XP: " + wolfcoins.xpList[whisperSender] + ")", group);
                                        //}
                                        //else
                                        //{
                                        //    string myClass = wolfcoins.determineClass(whisperSender);
                                        //    Whisper(whisperSender, "You are a Level " + myLevel + " " + myClass + " (Total XP: " + wolfcoins.xpList[whisperSender] + ")", group);
                                        //}

                                        if (wolfcoins.classList.Keys.Contains(whisperSender.ToLower()))
                                        {
                                            string myClass = wolfcoins.determineClass(whisperSender);
                                            Whisper(whisperSender, "You are a Level " + myLevel + " " + myClass + ", and you are Prestige Level " + myPrestige + ". (Total XP: " + wolfcoins.xpList[whisperSender] + " | XP To Next Level: " + xpToNextLevel + ")", group);
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "You are Level " + myLevel + " (Total XP: " + wolfcoins.xpList[whisperSender] + " | XP To Next Level: " + xpToNextLevel + ")", group);
                                        }

                                    }
                                    else
                                    {
                                        Whisper(whisperSender, "You don't have any XP yet! Hang out in chat during the livestream to earn XP & coins.", group);
                                    }
                                }
                            }
                            else if (whisperMessage.StartsWith("!shutdown"))
                            {
                                if (whisperSender != tokenData.BroadcastUser && whisperSender != tokenData.ChatUser)
                                    break;

                                string[] temp = whispers[1].Split(' ');
                                if (temp.Count() > 1)
                                {
                                    int numMinutes = -1;
                                    if (int.TryParse(temp[1], out numMinutes))
                                    {
                                        foreach (var player in groupFinder.queue)
                                        {
                                            Whisper(player.name, "Attention! Wolfpack RPG will be coming down for maintenance in about " + numMinutes + " minutes. If you are dungeoning while the bot shuts down, your progress may not be saved.", group);
                                        }
                                    }

                                }
                            }
                            else if (whisperMessage.StartsWith("!stats") || whisperMessage.StartsWith("stats"))
                            {
                                if (wolfcoins.coinList != null && wolfcoins.xpList != null)
                                {
                                    string[] temp = whispers[1].Split(' ');
                                    if (temp.Count() > 1)
                                    {
                                        string desiredUser = temp[1].ToLower();
                                        if (wolfcoins.xpList.ContainsKey(desiredUser) && wolfcoins.coinList.ContainsKey(desiredUser))
                                        {

                                            wolfcoins.RemoveCoins(whisperSender, pryCost.ToString());
                                            if (wolfcoins.Exists(wolfcoins.classList, desiredUser))
                                            {
                                                Whisper(whisperSender, "" + desiredUser + " is a Level " + wolfcoins.determineLevel(desiredUser) + " " + wolfcoins.determineClass(desiredUser) + " (" + wolfcoins.xpList[desiredUser] + " XP), Prestige Level " + wolfcoins.classList[desiredUser].prestige + ", and has " +
                                                    wolfcoins.coinList[desiredUser] + " Wolfcoins.", group);
                                            }
                                            else
                                            {
                                                Whisper(whisperSender, "" + desiredUser + " is Level " + " " + wolfcoins.determineLevel(desiredUser) + " (" + wolfcoins.xpList[desiredUser] + " XP) and has " +
                                                    wolfcoins.coinList[desiredUser] + " Wolfcoins.", group);
                                            }
                                            Whisper(whisperSender, "It cost you " + pryCost + " Wolfcoins to discover this information.", group);
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "User does not exist in database. You were charged no coins.", group);
                                        }
                                    }
                                    else if (wolfcoins.coinList.ContainsKey(whisperSender) && wolfcoins.xpList.ContainsKey(whisperSender))
                                    {

                                        int myLevel = wolfcoins.determineLevel(whisperSender);
                                        int myPrestige = -1;

                                        if (wolfcoins.classList.ContainsKey(whisperSender))
                                        {
                                            myPrestige = wolfcoins.classList[whisperSender].prestige;
                                        }

                                        int xpToNextLevel = wolfcoins.XpToNextLevel(whisperSender);
                                        Whisper(whisperSender, "You have: " + wolfcoins.coinList[whisperSender] + " coins.", group);
                                        if (wolfcoins.classList.Keys.Contains(whisperSender.ToLower()))
                                        {
                                            string myClass = wolfcoins.determineClass(whisperSender);
                                            Whisper(whisperSender, "You are a Level " + myLevel + " " + myClass + ", and you are Prestige Level " + myPrestige + ". (Total XP: " + wolfcoins.xpList[whisperSender] + " | XP To Next Level: " + xpToNextLevel + ")", group);
                                        }
                                        else
                                        {
                                            Whisper(whisperSender, "You are Level " + myLevel + " (Total XP: " + wolfcoins.xpList[whisperSender] + " | XP To Next Level: " + xpToNextLevel + ")", group);
                                        }
                                    }
                                    if (!(wolfcoins.coinList.ContainsKey(whisperSender)) || !(wolfcoins.xpList.ContainsKey(whisperSender)))
                                    {
                                        Whisper(whisperSender, "You either don't have coins or xp yet. Hang out in chat during the livestream to earn them!", group);
                                    }
                                }
                            }

                        }
                    }
                    #endregion
                    #region messageRegion
                    if (message.Length > 1)
                    {
                        if (message[0] != null && message[1] != null)
                        {
                            string[] first = message[1].Split(' ');
                            string sender = message[0];

                            if (sender == "twitchnotify" && first.Last() == "subscribed!")
                            {
                                // This code updates subcounter.txt for Subathon. Problem is having to recode the modifier update when the goal is met
                                //var text = File.ReadAllText(@"C:\Users\Lobos\Dropbox\Stream\subcounter.txt");
                                //int count = int.Parse(text);
                                //count--;
                                //File.WriteAllText(@"C:\Users\Lobos\Dropbox\Stream\subcounter.txt", count.ToString());
                                string newSub = first[0].ToLower();
                                if (!wolfcoins.subSet.Contains(newSub))
                                {
                                    wolfcoins.subSet.Add(newSub);
                                    Console.WriteLine("Added " + first[0] + " to the subs list.");
                                }
                            }
                            if (first[0] == "!stats" || first[0] == "!xp" || first[0] == "!lvl"
                                || first[0] == "!level" || first[0] == "!exp")
                            {
                                irc.sendChatMessage("/timeout " + sender + " 1");
                                Whisper(sender, "I see you're trying to check your stats! You'll need to WHISPER to me to get any information. Type '/w lobotjr' and then stats, xp, coins, etc. for that information.", group);
                                Whisper(sender, "Sorry for purging you. Just trying to do my job to keep chat clear! <3", group);
                            }

                            switch (first[0])
                            {
                                case "!nextaward":
                                    {
                                        if (sender != tokenData.BroadcastUser && sender != tokenData.ChatUser)
                                            break;
                                        double totalSec = (DateTime.Now - awardLast).TotalSeconds;
                                        int timeRemaining = (awardInterval * 60) - (int)(DateTime.Now - awardLast).TotalSeconds;
                                        int secondsRemaining = timeRemaining % 60;
                                        int minutesRemaining = timeRemaining / 60;
                                        irc.sendChatMessage(minutesRemaining + " minutes and " + secondsRemaining + " seconds until next coins/xp are awarded.");

                                    }
                                    break;

                                case "!setinterval":
                                    {
                                        if (sender != tokenData.BroadcastUser && sender != tokenData.ChatUser)
                                            break;

                                        int newAmount = 0;
                                        if (first.Count() > 1)
                                        {
                                            if (int.TryParse(first[1], out newAmount))
                                            {
                                                awardInterval = newAmount;
                                                irc.sendChatMessage("XP & Coins will now be awarded every " + newAmount + " minutes.");
                                            }
                                        }

                                    }
                                    break;

                                case "!setmultiplier":
                                    {
                                        if (sender != tokenData.BroadcastUser && sender != tokenData.ChatUser)
                                            break;

                                        int newAmount = 0;
                                        if (first.Count() > 1)
                                        {
                                            if (int.TryParse(first[1], out newAmount))
                                            {
                                                awardMultiplier = newAmount;
                                                irc.sendChatMessage(newAmount + "x XP & Coins will now be awarded.");
                                            }
                                        }

                                    }
                                    break;

                                //case "!endbet":
                                //    {
                                //        if (!betActive)
                                //            break;

                                //        if (wolfcoins.viewers.chatters.moderators.Contains(sender))
                                //        {
                                //            if (first.Count() > 1 && !betActive)
                                //            {
                                //                string result = first[1].ToLower();
                                //                if(result == "succeed")
                                //                {
                                //                    foreach(var element in betters)
                                //                    {
                                //                        //wolfcoins.AddCoins()
                                //                    }
                                //                }
                                //                else if(result == "fail")
                                //                {

                                //                }
                                //            }
                                //        }


                                //    } break;

                                //case "!closebet":
                                //    {
                                //        if(!betActive || !betsAllowed)
                                //            break;

                                //        if (wolfcoins.viewers.chatters.moderators.Contains(sender))
                                //        {
                                //            betsAllowed = false;
                                //            irc.sendChatMessage("Bets are now closed! Good luck FrankerZ");
                                //        }
                                //    } break;

                                //case "!startbet":
                                //    {

                                //        betStatement = "";
                                //        if (wolfcoins.viewers.chatters.moderators.Contains(sender))
                                //        {
                                //            if (first.Count() > 1 && !betActive)
                                //            {
                                //                betActive = true;
                                //                betsAllowed = true;
                                //                for (int i = 0; i < first.Count() - 1; i++)
                                //                {
                                //                    betStatement += first[i + 1];
                                //                }
                                //                irc.sendChatMessage("New bet started: " + betStatement + " Type '!bet succeed' or '!bet fail' to bet.");

                                //            }
                                //        }
                                //    } break;

                                case "!xpon":
                                    {
                                        wolfcoins.UpdateViewers(channel);
                                        if ((wolfcoins.viewers.chatters.moderators.Contains(sender) || sender == tokenData.BroadcastUser || sender == tokenData.ChatUser || sender == "lan5432") && !broadcasting)
                                        {
                                            broadcasting = true;
                                            awardLast = DateTime.Now;
                                            nextTournament = DateTime.Now.AddMinutes(15);

                                            irc.sendChatMessage("Wolfcoins & XP will be awarded.");
                                        }
                                    }
                                    break;

                                case "!xpoff":
                                    {
                                        wolfcoins.UpdateViewers(channel);
                                        if ((wolfcoins.viewers.chatters.moderators.Contains(sender) || sender == tokenData.BroadcastUser || sender == tokenData.ChatUser || sender == "lan5432") && broadcasting)
                                        {
                                            broadcasting = false;
                                            irc.sendChatMessage("Wolfcoins & XP will no longer be awarded.");
                                            wolfcoins.BackupData();
                                        }
                                    }
                                    break;

                                case "!setxp":
                                    {
                                        if (sender != tokenData.BroadcastUser && sender != tokenData.ChatUser)
                                            break;

                                        if (first.Length >= 3 && first[1] != null && first[2] != null)
                                        {
                                            if (int.TryParse(first[2], out int value))
                                            {
                                                int newXp = wolfcoins.SetXP(value, first[1], group);
                                                if (newXp != -1)
                                                {
                                                    irc.sendChatMessage("Set " + first[1] + "'s XP to " + newXp + ".");
                                                }
                                                else
                                                {
                                                    irc.sendChatMessage("Error updating XP amount.");
                                                }
                                            }
                                            else
                                            {
                                                irc.sendChatMessage("Invalid data provided for !setxp command.");
                                            }
                                        }
                                        else
                                        {
                                            irc.sendChatMessage("Not enough data provided for !setxp command.");
                                        }

                                    }
                                    break;

                                case "!grantxp":
                                    {
                                        if (sender != tokenData.BroadcastUser && sender != tokenData.ChatUser)
                                            break;

                                        if (first[0] != null && first[1] != null)
                                        {
                                            if (int.TryParse(first[1], out int value))
                                            {
                                                wolfcoins.AwardXP(value, group);
                                            }
                                            else
                                            {
                                                irc.sendChatMessage("Invalid data provided for !givexp command.");
                                            }
                                        }
                                        else
                                        {
                                            irc.sendChatMessage("Not enough data provided for !givexp command.");
                                        }

                                    }
                                    break;

                                case "!setcoins":
                                    {

                                    }
                                    break;

                                #region NormalBotStuff
                                //case "!hug":
                                //    {
                                //        irc.sendChatMessage("/me gives " + sender + " a big hug!");
                                //    } break;

                                //case "!playlist":
                                //    {
                                //        irc.sendChatMessage("Lobos' Spotify Playlist: http://open.spotify.com/user/1251282601/playlist/2j1FVSjJ4zdJiqGQgXgW3t");
                                //    } break;

                                //case "!opinion":
                                //    {
                                //        irc.sendChatMessage("Opinions go here: http:////i.imgur.com/3jRQ2fa.jpg");
                                //    } break;

                                //case "!quote":
                                //    {
                                //        string path = @"C:\Users\Owner\Dropbox\Stream\quotes.txt";
                                //        string myFile = "";
                                //        if (File.Exists(path))
                                //        {
                                //            myFile = File.ReadAllText(path);
                                //            string[] quotes = myFile.Split('\n');
                                //            int numQuotes = quotes.Length;
                                //            Random random = new Random();
                                //            int randomNumber = random.Next(0, numQuotes);
                                //            irc.sendChatMessage(quotes[randomNumber]);
                                //        }
                                //        else
                                //        {
                                //            irc.sendChatMessage("Quotes file does not exist.");
                                //        }
                                //    } break;

                                //case "!pun":
                                //    {
                                //        string path = @"C:\Users\Owner\Dropbox\Stream\puns.txt";
                                //        string myFile = "";
                                //        if (File.Exists(path))
                                //        {
                                //            myFile = File.ReadAllText(path);
                                //            string[] puns = myFile.Split('\n');
                                //            int numPuns = puns.Length;
                                //            Random random = new Random();
                                //            int randomNumber = random.Next(0, numPuns);
                                //            irc.sendChatMessage(puns[randomNumber]);
                                //        }
                                //        else
                                //        {
                                //            irc.sendChatMessage("Puns file does not exist.");
                                //        }
                                //    } break;

                                //case "!whisper":
                                //    {
                                //        if (group.connected)
                                //        {
                                //            group.sendChatMessage(".w " + sender + " Psssssst!");
                                //        }
                                //    } break;
                                #endregion
                                // remove coins from a target. Ex: !removecoins lobosjr 200
                                case "!removecoins":
                                    {

                                        if (first.Length < 3)
                                        {
                                            irc.sendChatMessage("Not enough information provided.");
                                            break;
                                        }

                                        string target = first[1];
                                        string coins = first[2];

                                        if (sender == tokenData.BroadcastUser || sender == tokenData.ChatUser)
                                        {
                                            if (wolfcoins.RemoveCoins(target, coins))
                                            {
                                                Console.WriteLine(sender + " removed " + coins + " coins from " + target + ".");
                                                irc.sendChatMessage(sender + " removed " + coins + " coins from " + target + ".");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Coin add operation failed with the following information: ");
                                                Console.WriteLine("Sender: " + sender);
                                                Console.WriteLine("Target: " + target);
                                                Console.WriteLine("Amount: " + coins + " coins.");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Non-Moderator attempted to add coins.");
                                            irc.sendChatMessage("Sorry, " + sender + ", AddCoins is a moderator-only command.");
                                        }

                                    }
                                    break;

                                case "!addcoins":
                                    {

                                        if (first.Length < 3)
                                        {
                                            irc.sendChatMessage("Not enough information provided.");
                                            break;
                                        }

                                        string target = first[1];
                                        string coins = first[2];

                                        if (sender == tokenData.BroadcastUser || sender == tokenData.ChatUser)
                                        {
                                            if (wolfcoins.AddCoins(target, coins))
                                            {
                                                Console.WriteLine(sender + " granted " + target + " " + coins + " coins.");
                                                irc.sendChatMessage(sender + " granted " + target + " " + coins + " coins.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Coin add operation failed with the following information: ");
                                                Console.WriteLine("Sender: " + sender);
                                                Console.WriteLine("Target: " + target);
                                                Console.WriteLine("Amount: " + coins + " coins.");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Non-Moderator attempted to add coins.");
                                            irc.sendChatMessage("Sorry, " + sender + ", AddCoins is a moderator-only command.");
                                        }

                                    }
                                    break;

                                default: break;

                            }
                        }
                    }
                    #endregion
                }

                Console.WriteLine("Connection terminated.");
                UpdateTokens(tokenData, clientData, true);
                connected = false;
                #endregion
            }
            else
            {
                #region TwitchPlaysDS

                irc.joinRoom("lobosjr");

                Process[] p = Process.GetProcessesByName("DARKSOULS");
                IntPtr h = (IntPtr)0;
                if (p.Length > 0)
                {
                    h = p[0].MainWindowHandle;
                    SetForegroundWindow(h);
                }

                while (connected)
                {
                    // message[0] has username, message[1] has message
                    string[] message = irc.readMessage();

                    if (message.Length > 1)
                    {
                        if (message[0] != null && message[1] != null)
                        {
                            string[] first = message[1].Split(' ');
                            string sender = message[0];
                            char[] keys = { };

                            const int MOVE_AMOUNT = 1000;
                            switch (first[0].ToLower())
                            {
                                // M preceds movement. MF = Move Forward, MB = Move Backwards, etc.
                                case "mf":
                                    {
                                        sendFor('W', MOVE_AMOUNT);
                                        Console.WriteLine("MF - Move Forward");
                                    }
                                    break;

                                case "mb":
                                    {
                                        sendFor('S', MOVE_AMOUNT);
                                        Console.WriteLine("MB - Move Back");
                                    }
                                    break;

                                case "ml":
                                    {
                                        sendFor('A', MOVE_AMOUNT);
                                        Console.WriteLine("ML - Move Left");
                                    }
                                    break;

                                case "mr":
                                    {
                                        sendFor('D', MOVE_AMOUNT);
                                        Console.WriteLine("MR - Move Right");
                                    }
                                    break;

                                // Camera Up, Down, Left, Right
                                case "cu":
                                    {
                                        sendFor('I', MOVE_AMOUNT);
                                        Console.WriteLine("CU - Camera Up");
                                    }
                                    break;

                                case "cd":
                                    {
                                        sendFor('K', MOVE_AMOUNT);
                                        Console.WriteLine("CD - Camera Down");
                                    }
                                    break;

                                case "cl":
                                    {
                                        sendFor('J', MOVE_AMOUNT);
                                    }
                                    break;

                                case "cr":
                                    {
                                        sendFor('L', MOVE_AMOUNT);
                                    }
                                    break;

                                // Lock on/off
                                case "l":
                                    {
                                        sendFor('O', 100);
                                    }
                                    break;

                                // Use item
                                case "u":
                                    {
                                        sendFor('E', 100);
                                    }
                                    break;
                                // 2h toggle
                                case "y":
                                    {
                                        sendFor(56, 100);
                                    }
                                    break;
                                // Attacks
                                case "r1":
                                    {
                                        sendFor('H', 100);
                                    }
                                    break;

                                case "r2":
                                    {
                                        sendFor('U', 100);
                                    }
                                    break;

                                case "l1":
                                    {
                                        sendFor(42, 100);
                                    }
                                    break;

                                case "l2":
                                    {
                                        sendFor(15, 100);
                                    }
                                    break;
                                // Rolling directions
                                case "rl":
                                    {
                                        keys = new char[] { 'A', ' ' };
                                        sendFor(keys, 100);
                                    }
                                    break;

                                case "rr":
                                    {
                                        keys = new char[] { 'D', ' ' };
                                        sendFor(keys, 100);
                                    }
                                    break;

                                case "rf":
                                    {
                                        keys = new char[] { 'W', ' ' };
                                        sendFor(keys, 100);
                                    }
                                    break;

                                case "rb":
                                    {
                                        keys = new char[] { 'S', ' ' };
                                        sendFor(keys, 100);
                                    }
                                    break;

                                case "x":
                                    {
                                        sendFor('Q', 100);
                                        sendFor(28, 100);
                                    }
                                    break;
                                // switch LH weap
                                case "dl":
                                    {
                                        sendFor('C', 100);
                                    }
                                    break;

                                case "dr":
                                    {
                                        sendFor('V', 100);
                                    }
                                    break;

                                case "du":
                                    {
                                        sendFor('R', 100);
                                    }
                                    break;

                                case "dd":
                                    {
                                        sendFor('F', 100);
                                    }
                                    break;

                                //case "just subscribed":
                                //    {
                                //        sendFor('G', 500);

                                //        sendFor(13, 100);
                                //    } break;

                                default: break;



                            }
                        }
                    }
                }

                Console.WriteLine("Connection terminated.");
                #endregion
            }
        }
        static Pet GrantPet(string playerName, Currency wolfcoins, Dictionary<int, Pet> petDatabase, IrcClient irc, IrcClient group)
        {
            List<Pet> toAward = new List<Pet>();
            // figure out the rarity of pet to give and build a list of non-duplicate pets to award
            int rarity = wolfcoins.classList[playerName].petEarned;
            foreach (var basePet in petDatabase)
            {
                if (basePet.Value.petRarity != rarity)
                    continue;

                bool alreadyOwned = false;


                foreach (var pet in wolfcoins.classList[playerName].myPets)
                {
                    if (pet.ID == basePet.Value.ID)
                    {
                        alreadyOwned = true;
                        break;
                    }

                }

                if (!alreadyOwned)
                {
                    toAward.Add(basePet.Value);
                }
            }
            // now that we have a list of eligible pets, randomly choose one from the list to award
            Pet newPet;

            if (toAward.Count > 0)
            {
                string toSend = "";
                Random RNG = new Random();
                int petToAward = RNG.Next(1, toAward.Count + 1);
                newPet = new Pet(toAward[petToAward - 1]);
                int sparklyCheck = RNG.Next(1, 101);
                bool firstPet = false;
                if (wolfcoins.classList[playerName].myPets.Count == 0)
                    firstPet = true;

                if (sparklyCheck == 1)
                    newPet.isSparkly = true;

                if (firstPet)
                {
                    newPet.isActive = true;
                    toSend = "You found your first pet! You now have a pet " + newPet.type + ". Whisper me !pethelp for more info.";
                }
                else
                {
                    toSend = "You found a new pet buddy! You earned a " + newPet.type + " pet!";
                }

                if (newPet.isSparkly)
                {
                    toSend += " WOW! And it's a sparkly version! Luck you!";
                }

                newPet.stableID = wolfcoins.classList[playerName].myPets.Count + 1;
                wolfcoins.classList[playerName].myPets.Add(newPet);




                Whisper(playerName, toSend, group);
                if (newPet.isSparkly)
                {
                    Console.WriteLine(DateTime.Now.ToString() + "WOW! " + ": " + playerName + " just found a SPARKLY pet " + newPet.name + "!");
                    irc.sendChatMessage("WOW! " + playerName + " just found a SPARKLY pet " + newPet.name + "! What luck!");
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": " + playerName + " just found a pet " + newPet.name + "!");
                    irc.sendChatMessage(playerName + " just found a pet " + newPet.name + "!");
                }

                if (wolfcoins.classList[playerName].myPets.Count == petDatabase.Count)
                {
                    Whisper(playerName, "You've collected all of the available pets! Congratulations!", group);
                }

                wolfcoins.classList[playerName].petEarned = -1;

                return newPet;
            }

            return new Pet();

        }

        static void UpdateTokens(TokenData tokenData, LobotJR.Shared.Client.ClientData clientData, bool force = false)
        {
            bool tokenUpdated = false;
            if (force || DateTime.Now >= tokenData.ChatToken.ExpirationDate)
            {
                tokenUpdated = true;
                try
                {
                    tokenData.ChatToken = AuthToken.Refresh(clientData.ClientId, clientData.ClientSecret, tokenData.ChatToken.RefreshToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred refreshing the chat token: {e.Message}\n{e.StackTrace}");
                }
            }
            if (force || DateTime.Now >= tokenData.BroadcastToken.ExpirationDate)
            {
                tokenUpdated = true;
                try
                {
                    tokenData.BroadcastToken = AuthToken.Refresh(clientData.ClientId, clientData.ClientSecret, tokenData.BroadcastToken.RefreshToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred refreshing the streamer token: {e.Message}\n{e.StackTrace}");
                }
            }
            if (tokenUpdated)
            {
                try
                {
                    FileUtils.WriteTokenData(tokenData);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred writing the updated token data: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        static string GetDungeonName(int dungeonID, Dictionary<int, string> dungeonList)
        {
            if (!dungeonList.ContainsKey(dungeonID))
                return "Invalid DungeonID";

            Dungeon tempDungeon = new Dungeon("content/dungeons/" + dungeonList[dungeonID]);
            return tempDungeon.dungeonName;
        }

        static string GetEligibleDungeons(string user, Currency wolfcoins, Dictionary<int, string> dungeonList)
        {
            string eligibleDungeons = "";
            int playerLevel = wolfcoins.determineLevel(user);
            List<Dungeon> dungeons = new List<Dungeon>();
            Dungeon tempDungeon;
            foreach (var id in dungeonList)
            {
                tempDungeon = new Dungeon("content/dungeons/" + dungeonList[id.Key]);
                tempDungeon.dungeonID = id.Key;
                dungeons.Add(tempDungeon);
            }

            if (dungeons.Count == 0)
                return eligibleDungeons;

            bool firstAdded = false;
            foreach (var dungeon in dungeons)
            {
                //if(dungeon.minLevel <= playerLevel)
                //{
                if (!firstAdded)
                {
                    firstAdded = true;
                }
                else
                {
                    eligibleDungeons += ",";
                }
                eligibleDungeons += dungeon.dungeonID;


                //}
            }
            return eligibleDungeons;
        }

        static int DetermineEligibility(string user, int dungeonID, Dictionary<int, string> dungeonList, int baseDungeonCost, Currency wolfcoins)
        {
            if (!dungeonList.ContainsKey(dungeonID))
                return -1;

            int playerLevel = wolfcoins.determineLevel(wolfcoins.xpList[user]);
            Dungeon tempDungeon = new Dungeon("content/dungeons/" + dungeonList[dungeonID]);

            if (wolfcoins.Exists(wolfcoins.coinList, user))
            {
                if (wolfcoins.coinList[user] < (baseDungeonCost + ((playerLevel - 3) * 10)))
                {
                    //not enough money
                    return -2;
                }
            }
            // no longer gate dungeons by level
            //if (tempDungeon.minLevel <= playerLevel)
            //    return 1;

            return 1;
        }

        static void WhisperPet(string user, Pet pet, IrcClient whisperClient, int detail)
        {
            const int LOW_DETAIL = 0;
            const int HIGH_DETAIL = 1;

            string name = pet.name;
            int stableID = pet.stableID;
            string rarity = "";
            switch (pet.petRarity)
            {
                case (Pet.QUALITY_COMMON):
                    {
                        rarity = "Common";
                    }
                    break;

                case (Pet.QUALITY_UNCOMMON):
                    {
                        rarity = "Uncommon";
                    }
                    break;

                case (Pet.QUALITY_RARE):
                    {
                        rarity = "Rare";
                    }
                    break;

                case (Pet.QUALITY_EPIC):
                    {
                        rarity = "Epic";
                    }
                    break;

                case (Pet.QUALITY_ARTIFACT):
                    {
                        rarity = "Legendary";
                    }
                    break;

                default:
                    {
                        rarity = "Error";
                    }
                    break;
            }

            List<string> stats = new List<string>();
            if (detail == HIGH_DETAIL)
                stats.Add("Level: " + pet.level + " | Affection: " + pet.affection + " | Energy: " + pet.hunger);

            bool active = pet.isActive;
            string status = "";
            string myStableID = "";
            if (active)
            {
                status = "Active";
                myStableID = "<[" + pet.stableID + "]> ";
            }
            else
            {
                status = "In the Stable";
                myStableID = "[" + pet.stableID + "] ";
            }

            Whisper(user, myStableID + name + " the " + pet.type + " (" + rarity + ") ", whisperClient);
            string sparkly = "";
            if (pet.isSparkly)
                sparkly = "Yes!";
            else
                sparkly = "No";
            if (detail == HIGH_DETAIL)
                Whisper(user, "Status: " + status + " | Sparkly? " + sparkly, whisperClient);

            foreach (var stat in stats)
            {
                Whisper(user, stat, whisperClient);
            }

        }

        static void WhisperFish(string user, List<Fish> myFish, IrcClient whisperClient)
        {
            int iterator = 0;
            foreach (var fish in myFish)
            {
                iterator++;
                Whisper(user, iterator + ": " + fish.name, whisperClient);
            }

            Whisper(user, "Type !fish # for more information on the particular type of fish.", whisperClient);
        }

        static void WhisperFish(string user, Dictionary<string, Fisherman> fishingList, int fishID, IrcClient whisperClient)
        {
            Fish myFish = fishingList[user].biggestFish[fishID - 1];
            string mySize = "";
            switch (myFish.sizeCategory)
            {
                case Fish.SIZE_TINY:
                    {
                        mySize = "Tiny";
                    }
                    break;

                case Fish.SIZE_SMALL:
                    {
                        mySize = "Small";
                    }
                    break;

                case Fish.SIZE_MEDIUM:
                    {
                        mySize = "Medium";
                    }
                    break;

                case Fish.SIZE_LARGE:
                    {
                        mySize = "Large";
                    }
                    break;

                case Fish.SIZE_HUGE:
                    {
                        mySize = "Huge";
                    }
                    break;
                default: break;
            }

            Whisper(user, "Name - " + myFish.name, whisperClient);
            Whisper(user, "Length - " + myFish.length + " in.", whisperClient);
            Whisper(user, "Weight - " + myFish.weight + " lbs.", whisperClient);
            Whisper(user, "Size Category - " + mySize, whisperClient);
            Whisper(user, myFish.flavorText, whisperClient);

        }


        static void WhisperItem(string user, Item itm, IrcClient whisperClient, Dictionary<int, Item> itemDatabase)
        {
            string name = itm.itemName;
            string type = "";
            int inventoryID = itm.inventoryID;
            switch (itm.itemType)
            {
                case (Item.TYPE_ARMOR):
                    {
                        type = "Armor";
                    }
                    break;

                case (Item.TYPE_WEAPON):
                    {
                        type = "Weapon";
                    }
                    break;

                case (Item.TYPE_OTHER):
                    {
                        type = "Misc. Item";
                    }
                    break;
                default:
                    {
                        type = "Broken";
                    }
                    break;
            }
            string rarity = "";
            switch (itm.itemRarity)
            {
                case Item.QUALITY_UNCOMMON:
                    {
                        rarity = "Uncommon";
                    }
                    break;

                case Item.QUALITY_RARE:
                    {
                        rarity = "Rare";
                    }
                    break;

                case Item.QUALITY_EPIC:
                    {
                        rarity = "Epic";
                    }
                    break;

                case Item.QUALITY_ARTIFACT:
                    {
                        rarity = "Artifact";
                    }
                    break;

                default:
                    {
                        rarity = "Broken";
                    }
                    break;
            }
            List<string> stats = new List<string>();

            if (itm.successChance > 0)
            {
                stats.Add("+" + itm.successChance + "% Success Chance");
            }

            if (itm.xpBonus > 0)
            {
                stats.Add("+" + itm.xpBonus + "% XP Bonus");
            }

            if (itm.coinBonus > 0)
            {
                stats.Add("+" + itm.coinBonus + "% Wolfcoin Bonus");
            }

            if (itm.itemFind > 0)
            {
                stats.Add("+" + itm.itemFind + "% Item Find");
            }

            if (itm.preventDeathBonus > 0)
            {
                stats.Add("+" + itm.preventDeathBonus + "% to Prevent Death");
            }

            bool active = itm.isActive;
            string status = "";
            if (active)
            {
                status = "(Equipped)";
            }
            else
            {
                status = "(Unequipped)";
            }

            Whisper(user, name + " (" + rarity + " " + type + ") " + status, whisperClient);
            Whisper(user, "Inventory ID: " + inventoryID, whisperClient);
            foreach (var stat in stats)
            {
                Whisper(user, stat, whisperClient);
            }
        }



        static int GrantItem(int id, Currency wolfcoins, string user, Dictionary<int, Item> itemDatabase)
        {
            string logPath = "dungeonlog.txt";

            if (id < 1)
                return -1;

            Item newItem = itemDatabase[id - 1];
            bool hasActiveItem = false;
            foreach (var item in wolfcoins.classList[user].myItems)
            {
                if (item.itemType == newItem.itemType && item.isActive)
                    hasActiveItem = true;
            }
            if (!hasActiveItem)
                newItem.isActive = true;

            wolfcoins.classList[user].totalItemCount++;
            newItem.inventoryID = wolfcoins.classList[user].totalItemCount;
            wolfcoins.classList[user].myItems.Add(newItem);
            wolfcoins.classList[user].itemEarned = -1;

            System.IO.File.AppendAllText(logPath, user + " looted a " + newItem.itemName + "!" + Environment.NewLine);
            Console.WriteLine(user + " just looted a " + newItem.itemName + "!");

            return newItem.itemID;
        }

        static void sendFor(char key, int ms)
        {
            // Send a keydown, leaving the key down indefinitely. It seems you have to do this for even
            // single inputs with shorter times because putting the keyup right after the keydown in the
            // buffer is too fast for Dark Souls
            var inputData = new INPUT[1];
            short scanCode = (short)MapVirtualKey(VkKeyScan(key), 0);
            inputData[0].type = INPUT_KEYBOARD;
            inputData[0].ki.wScan = scanCode;
            inputData[0].ki.dwFlags = KEYEVENTF_SCANCODE;
            SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));

            // A timer is probably more suitable
            Thread.Sleep(ms);

            // After waiting, send the keyup
            inputData[0].ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_SCANCODE;
            inputData[0].ki.time = 0;
            inputData[0].ki.dwExtraInfo = IntPtr.Zero;
            SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));
        }

        static void sendFor(short key, int ms)
        {
            // Send a keydown, leaving the key down indefinitely. It seems you have to do this for even
            // single inputs with shorter times because putting the keyup right after the keydown in the
            // buffer is too fast for Dark Souls
            var inputData = new INPUT[1];
            short scanCode = key;
            inputData[0].type = INPUT_KEYBOARD;
            inputData[0].ki.wScan = scanCode;
            inputData[0].ki.dwFlags = KEYEVENTF_SCANCODE;
            SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));

            // A timer is probably more suitable
            Thread.Sleep(ms);

            // After waiting, send the keyup
            inputData[0].ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_SCANCODE;
            inputData[0].ki.time = 0;
            inputData[0].ki.dwExtraInfo = IntPtr.Zero;
            SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));
        }

        // overloaded to handle more than one character send at a time
        static void sendFor(char[] key, int ms)
        {
            // Send a keydown, leaving the key down indefinitely. It seems you have to do this for even
            // single inputs with shorter times because putting the keyup right after the keydown in the
            // buffer is too fast for Dark Souls
            var inputData = new INPUT[1];
            short scanCode = (short)MapVirtualKey(VkKeyScan(key[0]), 0);
            inputData[0].type = INPUT_KEYBOARD;
            inputData[0].ki.wScan = scanCode;
            inputData[0].ki.dwFlags = KEYEVENTF_SCANCODE;
            SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));
            if (key.Length > 1)
            {
                inputData[0].ki.wScan = (short)MapVirtualKey(VkKeyScan(key[1]), 0);
                SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));
            }

            // A timer is probably more suitable
            Thread.Sleep(ms);

            // After waiting, send the keyup
            inputData[0].ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_SCANCODE;
            inputData[0].ki.time = 0;
            inputData[0].ki.dwExtraInfo = IntPtr.Zero;
            SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));
            if (key.Length > 1)
            {
                inputData[0].ki.wScan = scanCode;
                SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));
            }

        }
    }

}
