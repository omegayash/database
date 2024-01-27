using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using igf.DataAccessLayer.Contracts;
using igf.DependencyInterface;
using igf.Utility;
using SQLite;
using Xamarin.Forms;

namespace igf.DataAccessLayer
{
    public class DatabaseHandler
    {
        #region variable declarion

        /// <summary>
        /// locker is used to handle deadlock problem
        /// </summary>
        static object locker = UtilityFunction.GetLockerObject();
        //string m_strConnectionString;
        string[] m_strArrClassName;


        /// <summary>
        /// volatile field is used to reduces concurrency issue.
        /// volatile variable can be accessed by multiple threads
        /// </summary>
        volatile static DatabaseHandler dbInstance = null;

        #endregion

        /// <summary>
        /// Created single object of the class to handle singleton.
        /// </summary>

        public static DatabaseHandler DbInstance
        {
            get
            {
                lock (locker)
                {
                    return dbInstance ?? (dbInstance = new DatabaseHandler());
                }
            }
        }

        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <returns>The database.</returns>
        /// <param name="p_strPlatform">p_strPlatform string platform.</param>
        /// <param name="p_strConnectionString">p_strConnectionString string connection string.</param>
        /// <param name="p_strTableName">p_strTableName string table name.</param>
        public void CreateDatabase(string[] p_strTableName)
        {
            try
            {
                //	m_objSqlitePlateform = p_strPlatform;

                m_strArrClassName = p_strTableName;
                createDbConnection();
                createTables(m_strArrClassName);

            }
            catch (SQLiteException sqex)
            {
                UtilityFunction.CrashLog(sqex, this);
                throw sqex;
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                throw ex;

            }

        }

        /// <summary>
        /// create table
        /// </summary>
        /// <returns>will return 0 if table create successfully or else returns -1 </returns>
        public int createTables(string[] p_strArrClassNameString)
        {
            int iCount = -1;
            try
            {
                using (SQLiteConnection objSQLiteConnection = createDbConnection())
                {
                    if (objSQLiteConnection != null)
                    {
                        foreach (var strClassName in p_strArrClassNameString)
                        {
                            Type objClassType = Type.GetType(strClassName);
                            if (objClassType != null)
                            {
                                var result = objSQLiteConnection.CreateTable(objClassType);
                                iCount = 1;

                            }
                        }
                    }
                }
            }
            catch (SQLiteException sqex)
            {
                iCount = -1;
                UtilityFunction.CrashLog(sqex, this);
                throw sqex;
            }
            catch (Exception ex)
            {
                iCount = -1;
                UtilityFunction.CrashLog(ex, this);
                throw ex;
            }
            return iCount;
        }

        /// <summary>
        /// Create database connection
        /// </summary>
        /// <returns>SQLiteConnection object</returns>
        public SQLiteConnection createDbConnection()
        {
            SQLiteConnection objDbConnection = null;
            //objDbConnection = new SQLiteConnection((SQLite.Net.Interop.ISQLitePlatform)(m_objSqlitePlateform), m_strConnectionString);
            objDbConnection = DependencyService.Get<ISQLite>().GetConnection();

            //   objDbConnection = new SQLiteConnection(m_strConnectionString);
            return objDbConnection;
        }

        /// <summary>
        /// Get list of data from database
        /// </summary>
        /// <typeparam name="T">T is generic type which is used to pass table name</typeparam>
        /// <returns>list of enumerable data</returns>
        public List<T> getItems<T>() where T : class, new()
        {
            List<T> lItems = null;
            try
            {
                using (SQLiteConnection objSQLiteConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        lItems = (from item in objSQLiteConnection.Table<T>()
                                  select item).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                throw ex;
            }

            return lItems;
        }

        /// <summary>
        /// Get single data by passing primary key id
        /// </summary>
        /// <typeparam name="T">T is generic type which is used to pass table name</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>

        public T getItem<T>(string p_iId, bool p_isIntType) where T : class, INumericKey, new()
        {
            T tItem = null;
            int iId = 0;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        if (p_isIntType)
                        {
                            int.TryParse(p_iId, out iId);
                            if (iId != 0)
                            {
                                tItem = dbConnection.Table<T>().FirstOrDefault(item => item.ID == iId);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "getItem<T> Exception: " + ex.Message);
            }

            return tItem;
        }

        public T getAlphanumericIdData<T>(string p_iId, bool p_isIntType) where T : class, IAlphanumericKey, new()
        {
            T tItem = null;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        tItem = dbConnection.Table<T>().FirstOrDefault(item => item.ID == p_iId);
                    } 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "getItem<T> Exception: " + ex.Message);
            }

            return tItem;
        }
        /// <summary>
        /// This method is used to insert and update data by Table name
        /// </summary>
        /// <typeparam name="T">T is generic type which is used to pass table name</typeparam>
        /// <param name="item">T is generic type which is used to pass table name</param>
        /// <returns>count of table</returns>
        public int saveItem<T>(T p_tItem) where T : class, INumericKey, new()
        {
            int iCount = 0;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        var tData = getItem<T>(p_tItem.ID.ToString(), true);
                        iCount = tData != null ? dbConnection.Update(p_tItem) : dbConnection.Insert(p_tItem);
                    }
                }
            }
            catch (SQLiteException se)
            {
                UtilityFunction.CrashLog(se, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItem<T> SQLiteException: " + se.InnerException);
                throw se;
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItem<T> Exception: " + ex.Message);
                throw ex;
            }
            return iCount;
        }

        public int saveAlphanumericIdData<T>(T p_tItem) where T : class, IAlphanumericKey, new()
        {
            int iCount = 0;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        var tData = getAlphanumericIdData<T>(p_tItem.ID.ToString(), true);
                        iCount = tData != null ? dbConnection.Update(p_tItem) : dbConnection.Insert(p_tItem);
                    }
                }
            }
            catch (SQLiteException se)
            {
                UtilityFunction.CrashLog(se, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItem<T> SQLiteException: " + se.InnerException);
                throw se;
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItem<T> Exception: " + ex.Message);
                throw ex;
            }
            return iCount;
        }

        public int deleteAlphanumericIdData<T>(string p_iId) where T : class, IAlphanumericKey, new()
        {
            int iCount = 0;
            int iId = 0;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        int.TryParse(p_iId, out iId);
                        iCount = iId != 0 ? dbConnection.Delete<T>(iId) : dbConnection.Delete<T>(p_iId);
                    }
                }
            }
            catch (SQLiteException se)
            {
                UtilityFunction.CrashLog(se, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItem<T> SQLiteException: " + se.InnerException);
                throw se;
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItem<T> Exception: " + ex.Message);
                throw ex;
            }
            return iCount;
        }
        public int saveItems<T>(List<T> p_tItem) where T : class, INumericKey
        {
            int iCount = 0;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        var data = p_tItem as IEnumerable;
                        if (data != null)
                        {
                            iCount = dbConnection.InsertAll(p_tItem, true);

                        }
                    }
                }
            }
            catch (SQLiteException se)
            {
                UtilityFunction.CrashLog(se, this);
                // await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItems<T> SQLiteException: saveItems" + Newtonsoft.Json.JsonConvert.SerializeObject(p_tItem));
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                // await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "saveItems<T> SQLiteException: saveItems" + ex.Message + "  " + Newtonsoft.Json.JsonConvert.SerializeObject(p_tItem));
            }
            return iCount;
        }

        /// <summary>
        /// Updates the items.
        /// </summary>
        /// <returns>The items.</returns>
        /// <param name="p_tItem">P  t item.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public int updateItems<T>(List<T> p_tItem) where T : class, INumericKey, IAlphanumericKey
        {
            int iCount = 0;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        iCount = dbConnection.UpdateAll(p_tItem);
                    }
                }
            }
            catch (SQLiteException se)
            {
                UtilityFunction.CrashLog(se, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "updateItems<T> SQLiteException: updateItems " + se.InnerException);
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "updateItems<T> SQLiteException: " + ex.Message);
            }
            return iCount;
        }

        /// <summary>
        /// This method is used to delete single item by passing id.
        /// </summary>
        /// <typeparam name="T">T is generic type which is used to pass table name</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public int deleteItem<T>(string p_iId) where T : class, INumericKey, IAlphanumericKey
        {
            int iCount = 0;
            int iId = 0;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        int.TryParse(p_iId, out iId);
                        if (iId != 0)
                        {
                            iCount = dbConnection.Delete<T>(iId);
                        }
                        else
                        {
                            iCount = dbConnection.Delete<T>(p_iId);
                        }
                    }
                }
            }
            catch (SQLiteException se)
            {
                UtilityFunction.CrashLog(se, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "deleteItem<T> SQLiteException: " + se.InnerException);
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "deleteItem<T> Exception: " + ex.Message);
            }

            return iCount;
        }

        /// <summary>
        /// Deletes the items.
        /// </summary>
        /// <returns>The items.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public int deleteItems<T>() where T : new()
        {
            int iCount = 0;
            try
            {
                using (SQLiteConnection dbConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        iCount = dbConnection.DeleteAll<T>();
                    }
                }
            }
            catch (SQLiteException se)
            {
                UtilityFunction.CrashLog(se, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "deleteItems<T> SQLiteException: deleteItems" + se.InnerException);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "deleteItems<T> SQLiteException: deleteItems" + ex.Message);
            }
            return iCount;
        }

        public List<T> GetDynamicDocAsync<T>(Func<T, Boolean> predicate) where T : class, new()
        {
            List<T> Items = null;
            try
            {
                using (SQLiteConnection objSQLiteConnection = createDbConnection())
                {
                    lock (locker)
                    {
                        Items = (from item in objSQLiteConnection.Table<T>()
                                 select item).Where(predicate).ToList();
                    }
                }

                return Items;
            }
            catch (Exception ex)
            {
                UtilityFunction.CrashLog(ex, this);
                //await FileStorage.writeDataIntoLocalStorageFile("Logger", "Log", "deleteItems<T> SQLiteException: deleteItems" + ex.Message);
            }
            return Items;
        }
    }
}

