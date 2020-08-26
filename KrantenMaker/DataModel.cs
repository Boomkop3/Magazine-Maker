using System.Collections.ObjectModel;

namespace KrantenMaker
{
    class DataModel
    {
        public ObservableCollection<MagazinePage> magazinePages { get; set; }
        public DataModel()
        {
            magazinePages = new ObservableCollection<MagazinePage>();
        }
    }
}
