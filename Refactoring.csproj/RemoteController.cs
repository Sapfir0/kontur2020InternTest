using System;
using System.Collections.Generic;
using System.Text;

namespace Refactoring
{
    public class RemoteController
    {
        public Dictionary<String, int?> currentSettings = new Dictionary<string, int?>();
        public int volume = 1;
        public bool isOnline = false;
        private OptionsShower _optionsShower;

        public RemoteController()
        {
            _optionsShower = new OptionsShower(this);
        }

        public string Call(String command)
        {
            string subCommand = null;
            if (command.StartsWith("Options change"))
            {
                subCommand = command.Substring(14).Trim();
                command = "Options change";
            }

            switch (command)
            {
                case "Tv On":
                    isOnline = true;
                    break;
                case "Tv Off":
                    isOnline = false;
                    break;
                case "Volume Up":
                    volume += 10;
                    break;
                case "Volume Down":
                    volume -= 10;
                    break;
                case "Options change":
                    OptionsSwitch(subCommand);
                    break;
                case "Options show":
                    return _optionsShower.optionsShow(command);
                    break;
                default:
                    break;
            }

            return "";
        }

        private void OptionsSwitch(string command)
        {
            switch (command)
            {
                case "brightness up":
                    if (!currentSettings.ContainsKey("brightness"))
                    {
                        currentSettings.Add("brightness", 20 + 10);
                    }
                    else
                    {
                        currentSettings["brightness"] += 10;
                    }

                    break;
                case "brightness down":
                    if (!currentSettings.ContainsKey("brightness"))
                    {
                        currentSettings.Add("brightness", 20 - 10);
                    }
                    else
                    {
                        currentSettings["brightness"] -= 10;
                    }

                    break;
                case "contrast up":
                    if (!currentSettings.ContainsKey("contrast"))
                    {
                        currentSettings.Add("contrast", 20 + 10);
                    }
                    else
                    {
                        currentSettings["contrast"] += 10;
                    }

                    break;
                case "contrast down":
                    if (!currentSettings.ContainsKey("contrast"))
                    {
                        currentSettings.Add("contrast", 20 + 10);
                    }
                    else
                    {
                        currentSettings["contrast"] -= 10;
                    }

                    break;
                default:
                    break;
            }
        }
    }

    public class OptionsShower
    {
        private RemoteController _remoteController;

        public OptionsShower(RemoteController remoteController)
        {
            this._remoteController = remoteController;
        }

        public string optionsShow(String command)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Options:");
            int? brightness = 0;
            if (!_remoteController.currentSettings.ContainsKey("brightness"))
            {
                brightness = 20;
            }
            else
            {
                brightness = _remoteController.currentSettings["brightness"];
            }

            sb.AppendLine($"Volume {_remoteController.volume}");
            sb.AppendLine($"IsOnline {_remoteController.isOnline}");
            sb.AppendLine($"Brightness {brightness}");
            int? contrast;
            if (!_remoteController.currentSettings.ContainsKey("contrast"))
            {
                contrast = 20;
            }
            else
            {
                contrast = _remoteController.currentSettings["contrast"];
            }
            sb.AppendLine($"Contrast {contrast}");
            return sb.ToString();
        }
    }
    
    public class MainClass {
        static int Main() {
            var pult = new RemoteController();
            pult.Call("Options change brightness up");
            
            return 0;
            
        }
    }
}