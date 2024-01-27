using System;
using SQLite;

namespace xx.DataAccessLayer.Contracts
{
    public abstract class NumericKeyBase : INumericKey
    {
        public NumericKeyBase()
        {
        }

        /// <summary>
        /// Gets or sets the Database ID.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
    }

    public abstract class AlphanumericKey : IAlphanumericKey
    {
        public AlphanumericKey()
        {
        }

        /// <summary>
        /// Gets or sets the Database ID.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public string ID { get; set; }
    }
}
