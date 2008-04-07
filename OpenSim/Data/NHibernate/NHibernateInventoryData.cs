/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using libsecondlife;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Mapping.Attributes;
using OpenSim.Data;
using OpenSim.Framework;
using OpenSim.Framework.Console;
using Environment = NHibernate.Cfg.Environment;

namespace OpenSim.Data.NHibernate
{
    public class NHibernateInventoryData: IInventoryData
    {
        private static readonly log4net.ILog m_log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Configuration cfg;
        private ISessionFactory factory;

        /// <summary>
        /// Initialises the interface
        /// </summary>
        public void Initialise()
        {
            Initialise("Inventory.db", "Inventory");
        }

        public void Initialise(string dbfile, string dbname)
        {
                        // TODO: hard coding for sqlite based stuff to begin with, just making it easier to test

            // This is stubbing for now, it will become dynamic later and support different db backends
            cfg = new Configuration();
            cfg.SetProperty(Environment.ConnectionProvider, 
                            "NHibernate.Connection.DriverConnectionProvider");
            cfg.SetProperty(Environment.Dialect, 
                            "NHibernate.Dialect.SQLiteDialect");
            cfg.SetProperty(Environment.ConnectionDriver, 
                            "NHibernate.Driver.SqliteClientDriver");
            cfg.SetProperty(Environment.ConnectionString,
                            "URI=file:" + dbfile + ",version=3");
            cfg.AddAssembly("OpenSim.Data.NHibernate");

            HbmSerializer.Default.Validate = true;
            //             using ( System.IO.MemoryStream stream = 
            //                     HbmSerializer.Default.Serialize(System.Reflection.Assembly.GetExecutingAssembly()))
            //                 cfg.AddInputStream(stream);
            
            //             new SchemaExport(cfg).Create(true, true);

            factory  = cfg.BuildSessionFactory();
        }

        /*****************************************************************
         *
         *   Basic CRUD operations on Data 
         * 
         ****************************************************************/

        // READ

        /// <summary>
        /// Returns an inventory item by its UUID
        /// </summary>
        /// <param name="item">The UUID of the item to be returned</param>
        /// <returns>A class containing item information</returns>
        public InventoryItemBase getInventoryItem(LLUUID item)
        {
            using(ISession session = factory.OpenSession()) {
                try {
                    return session.Load(typeof(InventoryItemBase), item.ToString()) as InventoryItemBase;
                } catch {
                    return null;
                }
            }
        }

        /// <summary>
        /// Creates a new inventory item based on item
        /// </summary>
        /// <param name="item">The item to be created</param>
        public void addInventoryItem(InventoryItemBase item)
        {
            if (!ExistsItem(item.inventoryID)) {
                using(ISession session = factory.OpenSession()) {
                    using(ITransaction transaction = session.BeginTransaction()) {
                        session.Save(item);
                        transaction.Commit();
                    }
                }
            } else {
                m_log.ErrorFormat("Attempted to add Inventory Item {0} that already exists, updating instead", item.inventoryID);
                updateInventoryItem(item);
            }
        }

        /// <summary>
        /// Updates an inventory item with item (updates based on ID)
        /// </summary>
        /// <param name="item">The updated item</param>
        public void updateInventoryItem(InventoryItemBase item)
        {
            if (ExistsItem(item.inventoryID)) {
                using(ISession session = factory.OpenSession()) {
                    using(ITransaction transaction = session.BeginTransaction()) {
                        session.Update(item);
                        transaction.Commit();
                    }
                }
            } else {
                m_log.ErrorFormat("Attempted to add Inventory Item {0} that already exists", item.inventoryID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void deleteInventoryItem(LLUUID itemID)
        {
            using(ISession session = factory.OpenSession()) {
                using(ITransaction transaction = session.BeginTransaction()) {
                    session.Delete(itemID.ToString());
                    transaction.Commit();
                }
            }
        }


        /// <summary>
        /// Returns an inventory folder by its UUID
        /// </summary>
        /// <param name="folder">The UUID of the folder to be returned</param>
        /// <returns>A class containing folder information</returns>
        public InventoryFolderBase getInventoryFolder(LLUUID folder)
        {
            using(ISession session = factory.OpenSession()) {
                try {
                    return session.Load(typeof(InventoryFolderBase), folder.ToString()) as InventoryFolderBase;
                } catch {
                    return null;
                }
            }
        }

        /// <summary>
        /// Creates a new inventory folder based on folder
        /// </summary>
        /// <param name="folder">The folder to be created</param>
        public void addInventoryFolder(InventoryFolderBase folder)
        {
            if (!ExistsFolder(folder.folderID)) {
                using(ISession session = factory.OpenSession()) {
                    using(ITransaction transaction = session.BeginTransaction()) {
                        session.Save(folder);
                        transaction.Commit();
                    }
                }
            } else {
                m_log.ErrorFormat("Attempted to add Inventory Folder {0} that already exists, updating instead", folder.folderID);
                updateInventoryFolder(folder);
            }
        }

        /// <summary>
        /// Updates an inventory folder with folder (updates based on ID)
        /// </summary>
        /// <param name="folder">The updated folder</param>
        public void updateInventoryFolder(InventoryFolderBase folder)
        {
            if (ExistsFolder(folder.folderID)) {
                using(ISession session = factory.OpenSession()) {
                    using(ITransaction transaction = session.BeginTransaction()) {
                        session.Update(folder);
                        transaction.Commit();
                    }
                }
            } else {
                m_log.ErrorFormat("Attempted to add Inventory Folder {0} that already exists", folder.folderID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        public void deleteInventoryFolder(LLUUID folderID)
        {
            using(ISession session = factory.OpenSession()) {
                using(ITransaction transaction = session.BeginTransaction()) {
                    session.Delete(folderID.ToString());
                    transaction.Commit();
                }
            }
        }

        // useful private methods
        private bool ExistsItem(LLUUID uuid)
        {
            return (getInventoryItem(uuid) != null) ? true : false;
        }

        private bool ExistsFolder(LLUUID uuid)
        {
            return (getInventoryFolder(uuid) != null) ? true : false;
        }

        public void Shutdown()
        {
            // TODO: DataSet commit
        }

        /// <summary>
        /// Closes the interface
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// The plugin being loaded
        /// </summary>
        /// <returns>A string containing the plugin name</returns>
        public string getName()
        {
            return "NHibernate Inventory Data Interface";
        }

        /// <summary>
        /// The plugins version
        /// </summary>
        /// <returns>A string containing the plugin version</returns>
        public string getVersion()
        {
            Module module = GetType().Module;
            string dllName = module.Assembly.ManifestModule.Name;
            Version dllVersion = module.Assembly.GetName().Version;


            return
                string.Format("{0}.{1}.{2}.{3}", dllVersion.Major, dllVersion.Minor, dllVersion.Build,
                              dllVersion.Revision);
        }

        // Move seems to be just update

        public void moveInventoryFolder(InventoryFolderBase folder)
        {
            updateInventoryFolder(folder);
        }

        public void moveInventoryItem(InventoryItemBase item)
        {
            updateInventoryItem(item);
        }
        


        /// <summary>
        /// Returns a list of inventory items contained within the specified folder
        /// </summary>
        /// <param name="folderID">The UUID of the target folder</param>
        /// <returns>A List of InventoryItemBase items</returns>
        public List<InventoryItemBase> getInventoryInFolder(LLUUID folderID)
        {
            using(ISession session = factory.OpenSession()) {
                try {
                    IQuery query = session.CreateQuery("from InventoryItems i where i.parentFolderID = :parent");
                    query.SetString("parent", folderID.ToString());
                    List<InventoryItemBase> list = new List<InventoryItemBase>();
                    foreach (InventoryItemBase item in query.List())
                    {
                        list.Add(item);
                    }
                    return list;
                } catch {
                    return new List<InventoryItemBase>();
                }
            }
        }

        public List<InventoryFolderBase> getUserRootFolders(LLUUID user)
        {
            return new List<InventoryFolderBase>();
        }

        // see InventoryItemBase.getUserRootFolder
        public InventoryFolderBase getUserRootFolder(LLUUID user)
        {
//             using(ISession session = factory.OpenSession()) {
//                 try {
//                     IQuery query = session.CreateQuery("from InventoryItems i where i.parentFolderID = :parent");
//                     query.SetString("parent", folderID.ToString());
//                     List<InventoryItemBase> list = new List<InventoryItemBase>();
//                     foreach (InventoryItemBase item in query.List())
//                     {
//                         list.Add(item);
//                     }
//                     return list;
//                 } catch {
//                     return new List<InventoryItemBase>();
//                 }
//            }
            return new InventoryFolderBase();
        }
        
        /// <summary>
        /// Append a list of all the child folders of a parent folder 
        /// </summary>
        /// <param name="folders">list where folders will be appended</param>
        /// <param name="parentID">ID of parent</param>
        protected void getInventoryFolders(ref List<InventoryFolderBase> folders, LLUUID parentID)
        {
            using(ISession session = factory.OpenSession()) {
                try {
                    IQuery query = session.CreateQuery("from InventoryFolders i where i.parentFolderID = :parent");
                    query.SetString("parent", parentID.ToString());
                    foreach (InventoryFolderBase item in query.List())
                    {
                        folders.Add(item);
                    }
                } catch {
                    
                }
            }
        }

        /// <summary>
        /// Returns a list of inventory folders contained in the folder 'parentID'
        /// </summary>
        /// <param name="parentID">The folder to get subfolders for</param>
        /// <returns>A list of inventory folders</returns>
        public List<InventoryFolderBase> getInventoryFolders(LLUUID parentID)
        {
            List<InventoryFolderBase> folders = new List<InventoryFolderBase>();
            getInventoryFolders(ref folders, Util.ToRawUuidString(parentID));
            return folders;
        }

        // See IInventoryData
        public List<InventoryFolderBase> getFolderHierarchy(LLUUID parentID)
        {
            List<InventoryFolderBase> folders = new List<InventoryFolderBase>();
            getInventoryFolders(ref folders, Util.ToRawUuidString(parentID));

            for (int i = 0; i < folders.Count; i++)
                getInventoryFolders(ref folders, Util.ToRawUuidString(folders[i].folderID));

            return folders;
        }
    }
}
