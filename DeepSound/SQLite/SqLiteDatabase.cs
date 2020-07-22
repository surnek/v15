using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AutoMapper;
using DeepSound.Activities.Chat;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Comments;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using Newtonsoft.Json;
using SQLite;
 
//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================
namespace DeepSound.SQLite
{
    public class SqLiteDatabase : IDisposable
    {
        //############# DON'T MODIFY HERE #############
        private static readonly string Folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        public static readonly string PathCombine = Path.Combine(Folder, "DeepSoundMusic.db");
        private SQLiteConnection Connection;

        //Open Connection in Database
        //*********************************************************

        #region Connection

        private SQLiteConnection OpenConnection()
        {
            try
            {
                Connection = new SQLiteConnection(PathCombine);
                return Connection;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public void CheckTablesStatus()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    Connection.CreateTable<DataTables.LoginTb>();
                    Connection.CreateTable<DataTables.SettingsTb>();
                    Connection.CreateTable<DataTables.LibraryItemTb>();
                    Connection.CreateTable<DataTables.InfoUsersTb>();
                    Connection.CreateTable<DataTables.GenresTb>();
                    Connection.CreateTable<DataTables.PriceTb>();
                    Connection.CreateTable<DataTables.SharedTb>();
                    Connection.CreateTable<DataTables.LatestDownloadsTb>();
                    Connection.CreateTable<DataTables.LastChatTb>();
                    Connection.CreateTable<DataTables.MessageTb>();
                    Connection.Dispose();
                    Connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Close Connection in Database
        public void Dispose()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    Connection.Dispose();
                    Connection.Close();
                    GC.SuppressFinalize(this);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ClearAll()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    Connection.DeleteAll<DataTables.LoginTb>();
                    Connection.DeleteAll<DataTables.SettingsTb>(); 
                    Connection.DeleteAll<DataTables.LibraryItemTb>(); 
                    Connection.DeleteAll<DataTables.InfoUsersTb>(); 
                    Connection.DeleteAll<DataTables.GenresTb>(); 
                    Connection.DeleteAll<DataTables.PriceTb>(); 
                    Connection.DeleteAll<DataTables.SharedTb>(); 
                    Connection.DeleteAll<DataTables.LatestDownloadsTb>(); 
                    Connection.DeleteAll<DataTables.LastChatTb>();
                    Connection.DeleteAll<DataTables.MessageTb>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Delete table
        public void DropAll()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    Connection.DropTable<DataTables.LoginTb>();
                    Connection.DropTable<DataTables.SettingsTb>();
                    Connection.DropTable<DataTables.LibraryItemTb>();
                    Connection.DropTable<DataTables.InfoUsersTb>();
                    Connection.DropTable<DataTables.GenresTb>();
                    Connection.DropTable<DataTables.PriceTb>();
                    Connection.DropTable<DataTables.SharedTb>();
                    Connection.DropTable<DataTables.LatestDownloadsTb>(); 
                    Connection.DropTable<DataTables.LastChatTb>();
                    Connection.DropTable<DataTables.MessageTb>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion Connection
         
        //############# CONNECTION #############

        //########################## End SQLite_Entity ##########################

        //Start SQL_Commander >>  General
        //*********************************************************

        #region General

        public void InsertRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    Connection?.Insert(row);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void UpdateRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    Connection?.Update(row);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void DeleteRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    Connection?.Delete(row);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void InsertListOfRows(List<object> row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    Connection?.InsertAll(row);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion General

        //Start SQL_Commander >>  Custom
        //*********************************************************

        #region Login

        //Insert Or Update data Login
        public void InsertOrUpdateLogin_Credentials(DataTables.LoginTb db)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    var dataUser = Connection.Table<DataTables.LoginTb>().FirstOrDefault();
                    if (dataUser != null)
                    {
                        dataUser.UserId = UserDetails.UserId.ToString();
                        dataUser.AccessToken = UserDetails.AccessToken;
                        dataUser.Cookie = UserDetails.Cookie;
                        dataUser.Username = UserDetails.Username;
                        dataUser.Password = UserDetails.Password;
                        dataUser.Status = UserDetails.Status;
                        dataUser.Lang = AppSettings.Lang;
                        dataUser.DeviceId = UserDetails.DeviceId;
                        dataUser.Email = UserDetails.Email;
                         
                        Connection.Update(dataUser);
                    }
                    else
                    {
                        Connection.Insert(db);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Get data Login
        public DataTables.LoginTb Get_data_Login_Credentials()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null;

                    var dataUser = Connection.Table<DataTables.LoginTb>().FirstOrDefault();
                    if (dataUser != null)
                    {
                        UserDetails.Username = dataUser.Username;
                        UserDetails.FullName = dataUser.Username;
                        UserDetails.Password = dataUser.Password;
                        UserDetails.AccessToken = dataUser.AccessToken;
                        UserDetails.UserId = Convert.ToInt32(dataUser.UserId);
                        UserDetails.Status = dataUser.Status;
                        UserDetails.Cookie = dataUser.Cookie;
                        UserDetails.Email = dataUser.Email;
                        UserDetails.DeviceId = dataUser.DeviceId;
                        AppSettings.Lang = dataUser.Lang;

                        Current.AccessToken = dataUser.AccessToken;

                        ListUtils.DataUserLoginList.Clear();
                        ListUtils.DataUserLoginList.Add(dataUser);

                        return dataUser;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
          
        #endregion
         
        #region Settings

        public void InsertOrUpdateSettings(OptionsObject.Data settingsData)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    if (settingsData != null)
                    {
                        var select = Connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                        if (select == null)
                        {
                            var db = Mapper.Map<DataTables.SettingsTb>(settingsData);
                            Connection.Insert(db);
                        }
                        else
                        {
                            select = Mapper.Map<DataTables.SettingsTb>(settingsData);
                            Connection.Update(select);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Get Settings
        public OptionsObject.Data GetSettings()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var select = Connection?.Table<DataTables.SettingsTb>().FirstOrDefault();
                    if (select != null)
                    {
                        OptionsObject.Data db = Mapper.Map<OptionsObject.Data>(select);
                        if (db != null)
                        {
                            ListUtils.SettingsSiteList = null;
                            ListUtils.SettingsSiteList = db;

                            return db;
                        } 
                    }
                    return null;
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        #endregion
         
        #region Library Item

        //Insert data LibraryItem
        public void InsertLibraryItem(Classes.LibraryItem libraryItem)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    if (libraryItem == null) return;

                    var select = Connection.Table<DataTables.LibraryItemTb>().FirstOrDefault(a => a.SectionId == libraryItem.SectionId);
                    if (select != null)
                    {
                        select.SongsCount = libraryItem.SongsCount;
                        select.BackgroundImage = libraryItem.BackgroundImage;
                        Connection.Update(select);
                    }
                    else
                    {
                        DataTables.LibraryItemTb item = new DataTables.LibraryItemTb
                        {
                            SectionId = libraryItem.SectionId,
                            SectionText = libraryItem.SectionText,
                            SongsCount = libraryItem.SongsCount,
                            BackgroundImage = libraryItem.BackgroundImage
                        };
                        Connection.Insert(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Insert data LibraryItem
        public void InsertLibraryItem(ObservableCollection<Classes.LibraryItem> libraryList)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    if (libraryList?.Count == 0)
                        return;

                    if (libraryList != null)
                    {
                        foreach (var libraryItem in libraryList)
                        {
                            var select = Connection.Table<DataTables.LibraryItemTb>().FirstOrDefault(a => a.SectionId == libraryItem.SectionId);
                            if (select != null)
                            {
                                select.SectionId = libraryItem.SectionId;
                                select.SectionText = libraryItem.SectionText;
                                select.SongsCount = libraryItem.SongsCount;
                                select.BackgroundImage = libraryItem.BackgroundImage;

                                Connection.Update(select);
                            }
                            else
                            {
                                DataTables.LibraryItemTb item = new DataTables.LibraryItemTb
                                {
                                    SectionId = libraryItem.SectionId,
                                    SectionText = libraryItem.SectionText,
                                    SongsCount = libraryItem.SongsCount,
                                    BackgroundImage = libraryItem.BackgroundImage
                                };
                                Connection.Insert(item);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Get data LibraryItem
        public ObservableCollection<DataTables.LibraryItemTb> Get_LibraryItem()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<DataTables.LibraryItemTb>();

                    var select = Connection.Table<DataTables.LibraryItemTb>().OrderBy(a => a.SectionId).ToList();
                    if (select.Count > 0)
                    {
                        return new ObservableCollection<DataTables.LibraryItemTb>(select);
                    }
                    else
                    {
                        return new ObservableCollection<DataTables.LibraryItemTb>();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ObservableCollection<DataTables.LibraryItemTb>();
            }
        }

        #endregion
         
        #region Genres

        //Insert data Genres
        public void InsertOrUpdate_Genres(ObservableCollection<GenresObject.DataGenres> listData)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                     
                    var result = Connection.Table<DataTables.GenresTb>().ToList();

                    List<DataTables.GenresTb> list = new List<DataTables.GenresTb>();
                    foreach (var cat in listData)
                    {
                        var item = new DataTables.GenresTb
                        {
                            Id = cat.Id,
                            CateogryName = cat.CateogryName,
                            Tracks = Convert.ToInt32(cat.Tracks),
                            Color = cat.Color,
                            BackgroundThumb = cat.BackgroundThumb,
                            Time = Convert.ToInt32(cat.Time),
                        };
                        list.Add(item);

                        var update = result.FirstOrDefault(a => a.Id == cat.Id);
                        if (update != null)
                        {
                            update = item;
                            Connection.Update(update); 
                        }
                    }
                      
                    if (list.Count <= 0) return;
                      
                    Connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (newItemList.Count > 0)
                        Connection.InsertAll(newItemList);
                     
                    result = Connection.Table<DataTables.GenresTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            Connection.Delete(delete);

                    Connection.Commit(); 
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Get List Genres
        public ObservableCollection<GenresObject.DataGenres> Get_GenresList()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<GenresObject.DataGenres>();

                    var result = Connection.Table<DataTables.GenresTb>().ToList();
                    if (result?.Count > 0)
                    {
                        var list = result.Select(cat => new GenresObject.DataGenres()
                        {
                            Id = cat.Id,
                            CateogryName = cat.CateogryName,
                            Tracks = cat.Tracks,
                            Color = cat.Color,
                            BackgroundThumb = cat.BackgroundThumb,
                            Time = cat.Time,
                        }).ToList();
                          
                        return new ObservableCollection<GenresObject.DataGenres>(list);
                    }
                    else
                    {
                        return new ObservableCollection<GenresObject.DataGenres>();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ObservableCollection<GenresObject.DataGenres>();
            }
        }

        #endregion
         
        #region My Profile

        //Insert Or Update data My Profile Table
        public void InsertOrUpdate_DataMyInfo(UserDataObject info)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    var resultInfoTb = Connection.Table<DataTables.InfoUsersTb>().FirstOrDefault(a => a.Id == info.Id);
                    if (resultInfoTb != null)
                    {
                        resultInfoTb = Mapper.Map<DataTables.InfoUsersTb>(info);
                         
                        resultInfoTb.Followers = JsonConvert.SerializeObject(info.Followers); 
                        resultInfoTb.Following = JsonConvert.SerializeObject(info.Following); 
                        resultInfoTb.Albums = JsonConvert.SerializeObject(info.Albums); 
                        resultInfoTb.Blocks = JsonConvert.SerializeObject(info.Blocks); 
                        resultInfoTb.Favourites = JsonConvert.SerializeObject(info.Favourites); 
                        resultInfoTb.RecentlyPlayed = JsonConvert.SerializeObject(info.RecentlyPlayed); 
                        resultInfoTb.Liked = JsonConvert.SerializeObject(info.Liked); 
                        resultInfoTb.Latestsongs = JsonConvert.SerializeObject(info.Latestsongs); 
                        resultInfoTb.TopSongs = JsonConvert.SerializeObject(info.TopSongs); 
                        resultInfoTb.Store = JsonConvert.SerializeObject(info.Store); 
                        resultInfoTb.Stations = JsonConvert.SerializeObject(info.Stations); 

                        Connection.Update(resultInfoTb);
                    }
                    else
                    {
                        var db = Mapper.Map<DataTables.InfoUsersTb>(info);

                        db.Followers = JsonConvert.SerializeObject(info.Followers);
                        db.Following = JsonConvert.SerializeObject(info.Following);
                        db.Albums = JsonConvert.SerializeObject(info.Albums);
                        db.Blocks = JsonConvert.SerializeObject(info.Blocks);
                        db.Favourites = JsonConvert.SerializeObject(info.Favourites);
                        db.RecentlyPlayed = JsonConvert.SerializeObject(info.RecentlyPlayed);
                        db.Liked = JsonConvert.SerializeObject(info.Liked);
                        db.Latestsongs = JsonConvert.SerializeObject(info.Latestsongs);
                        db.TopSongs = JsonConvert.SerializeObject(info.TopSongs);
                        db.Store = JsonConvert.SerializeObject(info.Store);
                        db.Stations = JsonConvert.SerializeObject(info.Stations);
                        
                        Connection.Insert(db);
                    }

                    UserDetails.Avatar = info.Avatar;
                    UserDetails.Cover = info.Cover;
                    UserDetails.Username = info.Username;
                    UserDetails.FullName = info.Name;
                    UserDetails.Email = info.Email;

                    ListUtils.MyUserInfoList = new ObservableCollection<UserDataObject> {info};
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Get data To My Profile Table
        public void GetDataMyInfo()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    var info = Connection.Table<DataTables.InfoUsersTb>().FirstOrDefault();
                    if (info != null)
                    {
                        var db = Mapper.Map<UserDataObject>(info);
                        if (db != null)
                        {
                            UserDataObject asd = db;
                            asd.Followers = new List<UserDataObject>();
                            asd.Following = new List<UserDataObject>();
                            asd.Albums = new List<DataAlbumsObject>();
                            asd.Blocks = new List<UserDataObject>();
                            asd.Favourites = new List<SoundDataObject>();
                            asd.RecentlyPlayed = new List<SoundDataObject>();
                            asd.Liked = new List<SoundDataObject>();
                            asd.Latestsongs = new List<SoundDataObject>();
                            asd.TopSongs = new List<SoundDataObject>();
                            asd.Store = new List<SoundDataObject>();
                            asd.Stations = new List<SoundDataObject>();

                            if (!string.IsNullOrEmpty(info.Followers))
                                asd.Followers = JsonConvert.DeserializeObject<List<UserDataObject>>(info.Followers);

                            if (!string.IsNullOrEmpty(info.Following))
                                asd.Following = JsonConvert.DeserializeObject<List<UserDataObject>>(info.Following);

                            if (!string.IsNullOrEmpty(info.Albums))
                                asd.Albums = JsonConvert.DeserializeObject<List<DataAlbumsObject>>(info.Albums);

                            if (!string.IsNullOrEmpty(info.Blocks))
                                asd.Blocks = JsonConvert.DeserializeObject<List<UserDataObject>>(info.Blocks);

                            if (!string.IsNullOrEmpty(info.Favourites))
                                asd.Favourites = JsonConvert.DeserializeObject<List<SoundDataObject>>(info.Favourites);

                            if (!string.IsNullOrEmpty(info.RecentlyPlayed))
                                asd.RecentlyPlayed = JsonConvert.DeserializeObject<List<SoundDataObject>>(info.RecentlyPlayed);

                            if (!string.IsNullOrEmpty(info.Liked))
                                asd.Liked = JsonConvert.DeserializeObject<List<SoundDataObject>>(info.Liked);

                            if (!string.IsNullOrEmpty(info.Latestsongs))
                                asd.Latestsongs = JsonConvert.DeserializeObject<List<SoundDataObject>>(info.Latestsongs);

                            if (!string.IsNullOrEmpty(info.TopSongs))
                                asd.TopSongs = JsonConvert.DeserializeObject<List<SoundDataObject>>(info.TopSongs);

                            if (!string.IsNullOrEmpty(info.Store))
                                asd.Store = JsonConvert.DeserializeObject<List<SoundDataObject>>(info.Store);

                            if (!string.IsNullOrEmpty(info.Stations))
                                asd.Store = JsonConvert.DeserializeObject<List<SoundDataObject>>(info.Stations);

                            UserDetails.Avatar = asd.Avatar;
                            UserDetails.Cover = asd.Cover;
                            UserDetails.Username = asd.Username;
                            UserDetails.FullName = asd.Name;
                            UserDetails.Email = asd.Email;

                            ListUtils.MyUserInfoList = new ObservableCollection<UserDataObject> {asd};
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Price

        //Insert data Price
        public void InsertOrUpdate_Price(ObservableCollection<PricesObject.DataPrice> listData)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    var result = Connection.Table<DataTables.PriceTb>().ToList();

                    List<DataTables.PriceTb> list = new List<DataTables.PriceTb>();
                    foreach (var data in listData)
                    {
                        var item = new DataTables.PriceTb
                        {
                            Id = data.Id,
                            Price = data.Price
                        };
                        list.Add(item);

                        var update = result.FirstOrDefault(a => a.Id == data.Id);
                        if (update != null)
                        {
                            update = item;
                            Connection.Update(update);
                        }
                    }
                     
                    if (list.Count <= 0) return;

                    Connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (newItemList.Count > 0)
                        Connection.InsertAll(newItemList);
                     
                    result = Connection.Table<DataTables.PriceTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            Connection.Delete(delete);

                    Connection.Commit();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Get List Price
        public ObservableCollection<PricesObject.DataPrice> Get_PriceList()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<PricesObject.DataPrice> ();

                    var result = Connection.Table<DataTables.PriceTb>().ToList();
                    if (result?.Count > 0)
                    {
                        var list = result.Select(item => new PricesObject.DataPrice()
                        {
                            Id = item.Id,
                            Price = item.Price,
                        }).ToList();

                        return new ObservableCollection<PricesObject.DataPrice>(list);
                    }
                    else
                    {
                        return new ObservableCollection<PricesObject.DataPrice>();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ObservableCollection<PricesObject.DataPrice>();
            }
        }

        #endregion
         
        #region Shared Sound

        //Insert Or Update Shared Sound
        public void InsertOrUpdate_SharedSound(SoundDataObject info)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    if (info != null)
                    {
                        var select = Connection.Table<DataTables.SharedTb>().FirstOrDefault(a => a.Id == info.Id);
                        if (select != null)
                        {
                            select = Mapper.Map<DataTables.SharedTb>(info);
                            select.Publisher = JsonConvert.SerializeObject(info.Publisher);
                            select.TagsArray = JsonConvert.SerializeObject(info.TagsArray);
                            select.TagsFiltered = JsonConvert.SerializeObject(info.TagsFiltered);
                            select.SongArray = JsonConvert.SerializeObject(info.SongArray);
                            select.Comments = JsonConvert.SerializeObject(info.Comments);
                            Connection.Update(select);
                        }
                        else
                        {
                            var db = Mapper.Map<DataTables.SharedTb>(info);
                            db.Publisher = JsonConvert.SerializeObject(info.Publisher);
                            db.TagsArray = JsonConvert.SerializeObject(info.TagsArray);
                            db.TagsFiltered = JsonConvert.SerializeObject(info.TagsFiltered);
                            db.SongArray = JsonConvert.SerializeObject(info.SongArray);
                            db.Comments = JsonConvert.SerializeObject(info.Comments);
                            Connection.Insert(db);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Get Shared Sound
        public ObservableCollection<SoundDataObject> Get_SharedSound()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<SoundDataObject>();

                    var select = Connection.Table<DataTables.SharedTb>().ToList();
                    if (select.Count > 0)
                    {
                        var list = new ObservableCollection<SoundDataObject>();
                        foreach (var item in select)
                        {
                            var db = Mapper.Map<SoundDataObject>(item);
                            db.IsPlay = false;

                            if (!string.IsNullOrEmpty(item.Publisher))
                                db.Publisher = JsonConvert.DeserializeObject<UserDataObject>(item.Publisher);

                            if (!string.IsNullOrEmpty(item.TagsArray)) 
                                db.TagsArray = JsonConvert.DeserializeObject<List<string>>(item.TagsArray);

                            if (!string.IsNullOrEmpty(item.TagsFiltered)) 
                                db.TagsFiltered = JsonConvert.DeserializeObject<List<string>>(item.TagsFiltered);

                            if (!string.IsNullOrEmpty(item.SongArray)) 
                                db.SongArray = JsonConvert.DeserializeObject<SongArray>(item.SongArray);

                            if (!string.IsNullOrEmpty(item.TagsFiltered))
                                db.Comments = JsonConvert.DeserializeObject<List<CommentsDataObject>>(item.Comments);
                            
                            list.Add(db);
                        }

                        return list;
                    }
                    else
                    {
                        return new ObservableCollection<SoundDataObject>();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ObservableCollection<SoundDataObject>();
            }
        }

        #endregion

        #region LatestDownloads Sound

        //Insert Or Update Latest Downloads Sound
        public void InsertOrUpdate_LatestDownloadsSound(SoundDataObject info)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    if (info != null)
                    {
                        var select = Connection.Table<DataTables.LatestDownloadsTb>().FirstOrDefault(a => a.Id == info.Id);
                        if (select != null)
                        {
                            select = Mapper.Map<DataTables.LatestDownloadsTb>(info);
                            select.Publisher = JsonConvert.SerializeObject(info.Publisher);
                            select.TagsArray = JsonConvert.SerializeObject(info.TagsArray);
                            select.TagsFiltered = JsonConvert.SerializeObject(info.TagsFiltered);
                            select.SongArray = JsonConvert.SerializeObject(info.SongArray);
                            select.Comments = JsonConvert.SerializeObject(info.Comments);
                            Connection.Update(select);
                        }
                        else
                        {
                            var db = Mapper.Map<DataTables.LatestDownloadsTb>(info);
                            db.Publisher = JsonConvert.SerializeObject(info.Publisher);
                            db.TagsArray = JsonConvert.SerializeObject(info.TagsArray);
                            db.TagsFiltered = JsonConvert.SerializeObject(info.TagsFiltered);
                            db.SongArray = JsonConvert.SerializeObject(info.SongArray);
                            db.Comments = JsonConvert.SerializeObject(info.Comments);
                            Connection.Insert(db);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public void InsertOrUpdate_LatestDownloadsSound(long soundId, string soundPath)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    var select = Connection.Table<DataTables.LatestDownloadsTb>().FirstOrDefault(a => a.Id == soundId);
                    if (select != null)
                    {
                        //select.SoundName = soundId + ".mp3";
                        select.AudioLocation = soundPath;
                        Connection.Update(select);
                    } 
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception); 
            }
        }
         
        //Get LatestDownloads Sound
        public ObservableCollection<SoundDataObject> Get_LatestDownloadsSound()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<SoundDataObject>(); 

                    var select = Connection.Table<DataTables.LatestDownloadsTb>().ToList();
                    if (select.Count > 0)
                    {
                        var list = new ObservableCollection<SoundDataObject>();
                        foreach (var item in select)
                        {
                            var db = Mapper.Map<SoundDataObject>(item);
                            db.IsPlay = false;
                             
                            if (!string.IsNullOrEmpty(item.Publisher))
                                db.Publisher = JsonConvert.DeserializeObject<UserDataObject>(item.Publisher);

                            if (!string.IsNullOrEmpty(item.TagsArray))
                                db.TagsArray = JsonConvert.DeserializeObject<List<string>>(item.TagsArray);

                            if (!string.IsNullOrEmpty(item.TagsFiltered))
                                db.TagsFiltered = JsonConvert.DeserializeObject<List<string>>(item.TagsFiltered);

                            if (!string.IsNullOrEmpty(item.SongArray))
                                db.SongArray = JsonConvert.DeserializeObject<SongArray>(item.SongArray);

                            if (!string.IsNullOrEmpty(item.TagsFiltered))
                                db.Comments = JsonConvert.DeserializeObject<List<CommentsDataObject>>(item.Comments);

                            list.Add(db);
                        }

                        return list;
                    }
                    else
                    {
                        return new ObservableCollection<SoundDataObject>();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ObservableCollection<SoundDataObject>();
            }
        }

        //Remove LatestDownloads Sound
        public void Remove_LatestDownloadsSound(int soundId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    var select = Connection.Table<DataTables.LatestDownloadsTb>().FirstOrDefault(a => a.Id == soundId);
                    if (select != null)
                    {
                        Connection.Delete(select);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public SoundDataObject Get_LatestDownloadsSound(long soundId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null;

                    var select = Connection.Table<DataTables.LatestDownloadsTb>().FirstOrDefault(a => a.Id == soundId);
                    if (select != null)
                    {
                        var db = Mapper.Map<SoundDataObject>(select);
                        db.IsPlay = false;
                        if (!string.IsNullOrEmpty(select.Publisher))
                            db.Publisher = JsonConvert.DeserializeObject<UserDataObject>(select.Publisher);

                        if (!string.IsNullOrEmpty(select.TagsArray))
                            db.TagsArray = JsonConvert.DeserializeObject<List<string>>(select.TagsArray);

                        if (!string.IsNullOrEmpty(select.TagsFiltered))
                            db.TagsFiltered = JsonConvert.DeserializeObject<List<string>>(select.TagsFiltered);

                        if (!string.IsNullOrEmpty(select.SongArray))
                            db.SongArray = JsonConvert.DeserializeObject<SongArray>(select.SongArray);

                        if (!string.IsNullOrEmpty(select.TagsFiltered))
                            db.Comments = JsonConvert.DeserializeObject<List<CommentsDataObject>>(select.Comments);

                        return db;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        #endregion

        #region Last Chat

        //Insert data To Last Chat Table
        public void InsertOrReplaceLastChatTable(ObservableCollection<DataConversation> usersContactList)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var result = Connection.Table<DataTables.LastChatTb>().ToList();
                    List<DataTables.LastChatTb> list = new List<DataTables.LastChatTb>();
                    foreach (var user in usersContactList)
                    {
                        var item = new DataTables.LastChatTb
                        {
                            Id = user.User.Id.ToString(),
                            GetCountSeen = user.GetCountSeen,
                            GetLastMessage = JsonConvert.SerializeObject(user.GetLastMessage),
                            User = JsonConvert.SerializeObject(user.User),
                            ChatTime = user.ChatTime,
                        };

                        list.Add(item);

                        var update = result.FirstOrDefault(a => a.Id == user.User.Id.ToString());
                        if (update != null)
                        {
                            update = item;
                            update.GetCountSeen = user.GetCountSeen;

                            if (user.User != null)
                                update.User = JsonConvert.SerializeObject(user.User);

                            if (user.GetLastMessage != null)
                                update.GetLastMessage = JsonConvert.SerializeObject(user.GetLastMessage);

                            update.ChatTime = user.ChatTime;

                            Connection.Update(update);
                        }
                    }

                    if (list.Count > 0)
                    {
                        Connection.BeginTransaction();
                        //Bring new  
                        var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                        if (newItemList.Count > 0)
                        {
                            Connection.InsertAll(newItemList);
                        }

                        result = Connection.Table<DataTables.LastChatTb>().ToList();
                        var deleteItemList = result.Where(c => !list.Select(fc => fc.Id).Contains(c.Id)).ToList();
                        if (deleteItemList.Count > 0)
                            foreach (var delete in deleteItemList)
                                Connection.Delete(delete);

                        Connection.Commit();
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Get data To LastChat Table
        public ObservableCollection<DataConversation> GetAllLastChat()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var select = Connection.Table<DataTables.LastChatTb>().ToList();
                    if (select.Count > 0)
                    {
                        List<DataConversation> list = new List<DataConversation>();
                        foreach (var user in select)
                        {
                            var item = new DataConversation
                            {
                                GetCountSeen = user.GetCountSeen,
                                ChatTime = user.ChatTime,
                            };

                            if (user.GetLastMessage != null)
                                item.GetLastMessage = JsonConvert.DeserializeObject<ChatMessagesDataObject>(user.GetLastMessage);

                            if (user.User != null)
                                item.User = JsonConvert.DeserializeObject<UserDataObject>(user.User);
                             
                            list.Add(item);
                        }
                        return new ObservableCollection<DataConversation>(list);
                    }
                    else
                        return new ObservableCollection<DataConversation>();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ObservableCollection<DataConversation>();
            }
        }

        // Get data To LastChat Table By Id >> Load More
        public ObservableCollection<DataConversation> GetLastChatById(int id, int nSize)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var query = Connection.Table<DataTables.LastChatTb>().Where(w => w.AutoIdLastChat >= id)
                        .OrderBy(q => q.AutoIdLastChat).Take(nSize).ToList();
                    if (query.Count > 0)
                    {
                        var list = query.Select(user => new DataConversation
                        {
                            GetCountSeen = user.GetCountSeen,
                            ChatTime = user.ChatTime,
                            GetLastMessage = JsonConvert.DeserializeObject<ChatMessagesDataObject>(user.GetLastMessage),
                            User = JsonConvert.DeserializeObject<UserDataObject>(user.User),
                        }).ToList();

                        return list.Count > 0 ? new ObservableCollection<DataConversation>(list) : new ObservableCollection<DataConversation>();
                    }
                    else
                        return new ObservableCollection<DataConversation>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ObservableCollection<DataConversation>();
            }
        }

        //Remove data To LastChat Table
        public void DeleteUserLastChat(string userId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var user = Connection.Table<DataTables.LastChatTb>().FirstOrDefault(c => c.Id.ToString() == userId);
                    if (user != null)
                    {
                        Connection.Delete(user);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Clear All data LastChat
        public void ClearLastChat()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    Connection.DeleteAll<DataTables.LastChatTb>();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Message

        //Insert data To Message Table
        public void InsertOrReplaceMessages(ObservableCollection<ChatMessagesDataObject> messageList)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    List<DataTables.MessageTb> listOfDatabaseForInsert = new List<DataTables.MessageTb>();

                    // get data from database
                    var resultMessage = Connection.Table<DataTables.MessageTb>().ToList();
                    var listAllMessage = resultMessage.Select(messages => new ChatMessagesDataObject()
                    {
                        Id = messages.Id,
                        FromId = messages.FromId,
                        ToId = messages.ToId,
                        Text = messages.Text,
                        Seen = messages.Seen,
                        Time = messages.Time,
                        FromDeleted = messages.FromDeleted,
                        ToDeleted = messages.ToDeleted,
                        SentPush = messages.SentPush,
                        NotificationId = messages.NotificationId,
                        TypeTwo = messages.TypeTwo,
                        Image = messages.Image,
                        FullImage = messages.FullImage,
                        ApiPosition = JsonConvert.DeserializeObject<ApiPosition>(messages.ApiPosition),
                        ApiType = JsonConvert.DeserializeObject<ApiType>(messages.ApiType),
                    }).ToList();

                    foreach (var messages in messageList)
                    {
                        DataTables.MessageTb maTb = new DataTables.MessageTb()
                        {
                            Id = messages.Id,
                            FromId = messages.FromId,
                            ToId = messages.ToId,
                            Text = messages.Text,
                            Seen = messages.Seen,
                            Time = messages.Time,
                            FromDeleted = messages.FromDeleted,
                            ToDeleted = messages.ToDeleted,
                            SentPush = messages.SentPush,
                            NotificationId = messages.NotificationId,
                            TypeTwo = messages.TypeTwo,
                            Image = messages.Image,
                            FullImage = messages.FullImage,
                            ApiPosition = JsonConvert.SerializeObject(messages.ApiPosition),
                            ApiType = JsonConvert.SerializeObject(messages.ApiType),
                        };

                        var dataCheck = listAllMessage.FirstOrDefault(a => a.Id == messages.Id);
                        if (dataCheck != null)
                        {
                            var checkForUpdate = resultMessage.FirstOrDefault(a => a.Id == dataCheck.Id);
                            if (checkForUpdate != null)
                            {
                                checkForUpdate.Id = messages.Id;
                                checkForUpdate.FromId = messages.FromId;
                                checkForUpdate.ToId = messages.ToId;
                                checkForUpdate.Text = messages.Text;
                                checkForUpdate.Seen = messages.Seen;
                                checkForUpdate.Time = messages.Time;
                                checkForUpdate.FromDeleted = messages.FromDeleted;
                                checkForUpdate.ToDeleted = messages.ToDeleted;
                                checkForUpdate.SentPush = messages.SentPush;
                                checkForUpdate.NotificationId = messages.NotificationId;
                                checkForUpdate.TypeTwo = messages.TypeTwo;
                                checkForUpdate.Image = messages.Image;
                                checkForUpdate.FullImage = messages.FullImage;
                                checkForUpdate.ApiPosition = JsonConvert.SerializeObject(messages.ApiPosition);
                                checkForUpdate.ApiType = JsonConvert.SerializeObject(messages.ApiType);

                                Connection.Update(checkForUpdate);
                            }
                            else
                            {
                                listOfDatabaseForInsert.Add(maTb);
                            }
                        }
                        else
                        {
                            listOfDatabaseForInsert.Add(maTb);
                        }
                    }

                    Connection.BeginTransaction();

                    //Bring new  
                    if (listOfDatabaseForInsert.Count > 0)
                    {
                        Connection.InsertAll(listOfDatabaseForInsert);
                    }

                    Connection.Commit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Update one Messages Table
        public void InsertOrUpdateToOneMessages(ChatMessagesDataObject messages)
        {
            try
            {
                using (Connection = OpenConnection())
                {

                    var data = Connection.Table<DataTables.MessageTb>().FirstOrDefault(a => a.Id == messages.Id);
                    if (data != null)
                    {
                        data.Id = messages.Id;
                        data.FromId = messages.FromId;
                        data.ToId = messages.ToId;
                        data.Text = messages.Text;
                        data.Seen = messages.Seen;
                        data.Time = messages.Time;
                        data.FromDeleted = messages.FromDeleted;
                        data.ToDeleted = messages.ToDeleted;
                        data.SentPush = messages.SentPush;
                        data.NotificationId = messages.NotificationId;
                        data.TypeTwo = messages.TypeTwo;
                        data.Image = messages.Image;
                        data.FullImage = messages.FullImage;
                        data.ApiPosition = JsonConvert.SerializeObject(messages.ApiPosition);
                        data.ApiType = JsonConvert.SerializeObject(messages.ApiType);
                        Connection.Update(data);
                    }
                    else
                    {
                        DataTables.MessageTb mdb = new DataTables.MessageTb
                        {
                            Id = messages.Id,
                            FromId = messages.FromId,
                            ToId = messages.ToId,
                            Text = messages.Text,
                            Seen = messages.Seen,
                            Time = messages.Time,
                            FromDeleted = messages.FromDeleted,
                            ToDeleted = messages.ToDeleted,
                            SentPush = messages.SentPush,
                            NotificationId = messages.NotificationId,
                            TypeTwo = messages.TypeTwo,
                            Image = messages.Image,
                            FullImage = messages.FullImage,
                            ApiPosition = JsonConvert.SerializeObject(messages.ApiPosition),
                            ApiType = JsonConvert.SerializeObject(messages.ApiType),
                        };

                        //Insert  one Messages Table
                        Connection.Insert(mdb);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Get data To Messages
        public string GetMessagesList(long fromId, long toId, long beforeMessageId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var beforeQ = "";
                    if (beforeMessageId != 0)
                    {
                        beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                    }

                    var query = Connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + ")) " + beforeQ);
                    List<DataTables.MessageTb> queryList = query.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).OrderBy(q => q.Time).TakeLast(35).ToList();
                    if (queryList.Count > 0)
                    {
                        foreach (var m in queryList.Select(messages => new ChatMessagesDataObject
                        {
                            Id = messages.Id,
                            FromId = messages.FromId,
                            ToId = messages.ToId,
                            Text = messages.Text,
                            Seen = messages.Seen,
                            Time = messages.Time,
                            FromDeleted = messages.FromDeleted,
                            ToDeleted = messages.ToDeleted,
                            SentPush = messages.SentPush,
                            NotificationId = messages.NotificationId,
                            TypeTwo = messages.TypeTwo,
                            Image = messages.Image,
                            FullImage = messages.FullImage,
                            ApiPosition = JsonConvert.DeserializeObject<ApiPosition>(messages.ApiPosition),
                            ApiType = JsonConvert.DeserializeObject<ApiType>(messages.ApiType),
                        }))
                        {
                            if (beforeMessageId == 0)
                            {
                                if (MessagesBoxActivity.MAdapter != null)
                                {
                                    MessagesBoxActivity.MAdapter.MessageList.Add(m);

                                    int index = MessagesBoxActivity.MAdapter.MessageList.IndexOf(MessagesBoxActivity.MAdapter.MessageList.Last());
                                    if (index > -1)
                                    {
                                        MessagesBoxActivity.MAdapter.NotifyItemInserted(index);

                                        //Scroll Down >> 
                                        MessagesBoxActivity.GetInstance()?.ChatBoxRecyclerView.ScrollToPosition(index);
                                    }
                                }
                            }
                            else
                            {
                                MessagesBoxActivity.MAdapter?.MessageList.Insert(0, m);
                                MessagesBoxActivity.MAdapter?.NotifyItemInserted(MessagesBoxActivity.MAdapter.MessageList.IndexOf(MessagesBoxActivity.MAdapter.MessageList.FirstOrDefault()));

                                var index = MessagesBoxActivity.MAdapter?.MessageList.FirstOrDefault(a => a.Id == beforeMessageId);
                                if (index != null)
                                {
                                    MessagesBoxActivity.MAdapter?.NotifyItemChanged(MessagesBoxActivity.MAdapter.MessageList.IndexOf(index));
                                    //Scroll Down >> 
                                    MessagesBoxActivity.GetInstance()?.ChatBoxRecyclerView.ScrollToPosition(MessagesBoxActivity.MAdapter.MessageList.IndexOf(MessagesBoxActivity.MAdapter.MessageList.Last()));

                                }
                            }
                        }

                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "0";
            }
        }

        //Get data To where first Messages >> load more
        public List<DataTables.MessageTb> GetMessageList(long fromId, long toId, long beforeMessageId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var beforeQ = "";
                    if (beforeMessageId != 0)
                    {
                        beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                    }

                    var query = Connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + ")) " + beforeQ);
                    List<DataTables.MessageTb> queryList = query
                        .Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId)
                        .OrderBy(q => q.Time).TakeLast(35).ToList();
                    return queryList;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<DataTables.MessageTb>();
            }
        }

        //Remove data To Messages Table
        public void Delete_OneMessageUser(int messageId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var user = Connection.Table<DataTables.MessageTb>().FirstOrDefault(c => c.Id == messageId);
                    if (user != null)
                    {
                        Connection.Delete(user);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void DeleteAllMessagesUser(string fromId, string toId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var query = Connection.Query<DataTables.MessageTb>("Delete FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + "))");
                    Console.WriteLine(query);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Remove All data To Messages Table
        public void ClearAll_Messages()
        {
            try
            {
                Connection.DeleteAll<DataTables.MessageTb>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

    }
}