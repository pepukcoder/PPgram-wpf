using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPgram_desktop.MVVM.Model
{
    internal class UserModel
    {
        public int ID { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string AvatarSource { get; set; } = "";
        public string LastMessage { get; set; } = "";

        public ObservableCollection<ChatMessageModel> Messages { get; set; } = [];
    }
}
