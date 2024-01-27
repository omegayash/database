using System;
using System.Threading.Tasks;
using namespace.DataAccessLayer.Contracts;
using namespace.Utility;
using SQLite;

namespace xx.DataAccessLayer
{
    public class DownloadHandler
    {

        #region variable declarion
        static object locker = UtilityFunction.GetLockerObject();
        //SQLiteConnection objSQLiteConnection = null;
        #endregion

        public DownloadHandler()
        {
        }

        public int saveItemLanguage<T>(T p_tItem) where T : class, ILanguageKey, ILanguageID, new()
        {
            int iCount = 0;
            try
            {

                using (SQLiteConnection dbConnection = UtilityFunction.CreateDbConnection())
                {

                    lock (locker)
                    {
                        //dbConnection.BusyTimeout = TimeSpan.FromSeconds(5);
                        var tData = getItemLanguage<T>(p_tItem.languageKey.ToString(), p_tItem.language_id.ToString());
                        if (tData != null)
                        {

                            iCount = dbConnection.Update(p_tItem);
                            dbConnection.Close();
                            dbConnection.Dispose();
                        }
                        else
                        {
                            iCount = dbConnection.Insert(p_tItem);
                            dbConnection.Close();
                            dbConnection.Dispose();
                            //Console.WriteLine("Language " + iCount);

                        }

                        //  var select = dbConnection.Table<T>().Where(x => x.language_id == "1" && x.resource_key == "field.display.reporties").ToList();

                    }
                }
            }
            catch (SQLiteException)
            {
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItem<T> SQLiteException:  saveItemLanguage  " + se.InnerException);
                //throw se;
            }
            catch (Exception)
            {
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItem<T> Exception: " + ex.Message);
                // throw ex;
                return iCount = 0;
            }
            return iCount;
        }

        public T getItemLanguage<T>(string p_iId, string LangID) where T : class, ILanguageKey, ILanguageID, new()
        {
            T tItem = null;
            //string iId = "0";
            try
            {
                using (SQLiteConnection dbConnection = UtilityFunction.CreateDbConnection())
                {
                    lock (locker)
                    {
                        if (p_iId != "0")
                        {
                            tItem = dbConnection.Table<T>().FirstOrDefault(item => item.languageKey == p_iId && item.language_id == LangID);
                            dbConnection.Close();
                            dbConnection.Dispose();
                        }

                    }
                }
            }
            catch (Exception)
            {
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "getItem<T> Exception: " + ex.Message);
            }

            return tItem;
        }


    }
}
