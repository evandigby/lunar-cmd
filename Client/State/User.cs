namespace Client.State
{
    public class User : StateObject
    {
        private string _id = "";
        private string _name = "";
        private string _preferredUserName = "";

        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                NotifyStateChanged();
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyStateChanged();
            }
        }
        public string PreferredUserName
        {
            get => _preferredUserName;
            set
            {
                _preferredUserName = value;
                NotifyStateChanged();
            }
        }
    }
}
