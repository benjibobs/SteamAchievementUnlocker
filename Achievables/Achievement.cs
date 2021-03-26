namespace Achievables
{
    class Achievement
    {
        private string displayName;
        private string id;

        public Achievement(string displayName, string id)
        {
            this.displayName = displayName;
            this.id = id;
        }

        public string getDisplayName()
        {
            return displayName;
        }

        public string getId()
        {
            return id;
        }
    }
}
