using B2Lib.Client;

namespace B2Lib.SyncExtensions
{
    public static class B2FileItemBaseExtensions
    {
        public static bool Delete(this B2FileItemBase file)
        {
            return Utility.AsyncRunHelper(() => file.DeleteAsync());
        }
    }
}