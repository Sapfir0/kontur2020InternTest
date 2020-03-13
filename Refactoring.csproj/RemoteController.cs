using System;
using System.Collections.Generic;
using System.Text;

namespace Refactoring
{
    public class RemoteController
    {
        public Dictionary<String, int?> currentSettings = new Dictionary<string, int?>();
        public int volume = 30;
        public bool isOnline = false;
        private OptionsShower _optionsShower;

        public RemoteController()
        {
            _optionsShower = new OptionsShower(this);
            currentSettings.Add("brightness", 30);
            currentSettings.Add("contrast", 30);
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
                    return _optionsShower.OptionsShow(command);
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
                    currentSettings["brightness"] += 10;
                    break;
                case "brightness down":
                    currentSettings["brightness"] -= 10;
                    break;
                case "contrast up":
                    currentSettings["contrast"] += 10;
                    break;
                case "contrast down":
                    currentSettings["contrast"] -= 10;
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

        public string OptionsShow(string command)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Options:");
            _remoteController.currentSettings.TryGetValue("contrast", out var brightness);
            sb.AppendLine($"Volume {_remoteController.volume}");
            sb.AppendLine($"IsOnline {_remoteController.isOnline}");
            sb.AppendLine($"Brightness {brightness}");
            _remoteController.currentSettings.TryGetValue("contrast", out var contrast);
            sb.AppendLine($"Contrast {contrast}");
            return sb.ToString();
        }
    }
    
}