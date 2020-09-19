﻿using Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace GarouToremo
{
    class Program
    {
        const int FPS = 120;

        Cheats cheats;
        Overlay overlay;
        IHotkeyListenable hotkeyHandler;
        InputHistory p1InputHistory;
        InputHistory p2InputHistory;

        static void Main(string[] args)
        {
            new Program().Run();
        }

        public Program()
        {
            Mem mem = new Mem();

            if (!mem.OpenProcess("Garou")) {
                Console.WriteLine("Error to open Garou proccess. Is the game running?");
                Environment.Exit(1);
            }

            p1InputHistory = new InputHistory();
            p2InputHistory = new InputHistory();
            cheats = new Cheats(mem);
            overlay = new Overlay();
        }

        public void Run()
        {
            overlay.Run();
            this.overlay.InfoText = "GarouToremo is running";
            new Thread(this.CheatLoop).Start();

            string option = String.Empty;
            while (option != "q")
            {
                Console.Clear();
                Console.WriteLine("Enter m to menu or q to exit");
                option = Console.ReadLine().ToLower();

                if (option == "m")
                {
                    ShowMenu();
                }
            }
        }

        private void CheatLoop(Object o)
        {
            while (true)
            {
                cheats.SetHp(Player.P1, Cheats.MAX_HP);
                cheats.SetHp(Player.P2, Cheats.MAX_HP);
                cheats.SetPower(Player.P1, Cheats.MAX_POWER);
                cheats.SetPower(Player.P2, Cheats.MAX_POWER);
                cheats.SetTime(Cheats.MAX_TIME);

                byte currentP1Input = cheats.GetCurrentInputByte(Player.P1);
                p1InputHistory.AddInput(currentP1Input);
                overlay.effectiveInputsP1 = p1InputHistory.GetEffectiveInputs();

                byte currentP2Input = cheats.GetCurrentInputByte(Player.P2);
                p2InputHistory.AddInput(currentP2Input);
                overlay.effectiveInputsP2 = p2InputHistory.GetEffectiveInputs();


                if (hotkeyHandler != null)
                {
                    hotkeyHandler.Update();

                    if (hotkeyHandler.ResetPositionCenterPressed())
                    {
                        BackToCenter();
                        overlay.InfoText = "Position reseted to center";
                    }
                }
            }
        }

        private void BackToCenter()
        {
            cheats.SetScenarioPosition(Cheats.POSITION_X_CENTER_SCENARIO);
            cheats.SetPlayerPosition(Player.P1, Cheats.POSITION_X_CENTER_P1, Cheats.POSITION_Y_CENTER_P1);
            cheats.SetPlayerPosition(Player.P2, Cheats.POSITION_X_CENTER_P2, Cheats.POSITION_Y_CENTER_P2);
            Thread.Sleep(50);
            cheats.SetPlayerPosition(Player.P1, Cheats.POSITION_X_CENTER_P1, Cheats.POSITION_Y_CENTER_P1);
            cheats.SetPlayerPosition(Player.P2, Cheats.POSITION_X_CENTER_P2, Cheats.POSITION_Y_CENTER_P2);
        }

        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("Enter:");
            Console.WriteLine("1 - Toggle Show Inputs [{0}]", overlay.ShowInputHistory);
            Console.WriteLine("2 - Set hotkeys");
            Console.WriteLine("Any other key - Quit menu");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    overlay.ShowInputHistory = !overlay.ShowInputHistory;
                    break;
                case "2":
                    SetHotkeys();
                    break;
            }
        }

        private void SetHotkeys()
        {
            IHotkeyListenable newHotkeyHandler = ChooseHotkeyHandler();
            if(newHotkeyHandler != null)
            {
                if(hotkeyHandler != null)
                {
                    hotkeyHandler.Dispose();
                }
                hotkeyHandler = newHotkeyHandler;
                ConfigureHotkeyHandler();
            }
        }

        private IHotkeyListenable ChooseHotkeyHandler()
        {
            Console.Clear();
            Console.WriteLine("1 - Keyboard");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    return new KeyboardHotkey();
            }

            return null;
        }

        private void ConfigureHotkeyHandler()
        {
            Console.WriteLine("Set reset position Hotkey");
            hotkeyHandler.SetRestPositionHotkey();
        }
    }
}
