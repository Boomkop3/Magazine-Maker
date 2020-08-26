namespace KrantenMaker
{
    public class MagazinePage
    {
        public int id { get; set; }
        public string filename { get; set; }

        public MagazinePage(string filename)
        {
            this.id = Increment.value;
            this.filename = filename;
        }
    }
}
