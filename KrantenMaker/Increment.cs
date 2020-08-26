namespace KrantenMaker
{
    public static class Increment
    {
        private static int holder;
        static Increment()
        {
            holder = 0;
        }
        public static int value
        {
            get
            {
                return holder++;
            }
        }
    }
}
