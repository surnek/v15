using System;
using AutoMapper;
using AutoMapper.Configuration;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;

namespace DeepSound.Helpers.Utils
{
    public static class ClassMapper
    {
        public static void SetMappers()
        {
            try
            {
                var cfg = new MapperConfigurationExpression
                {
                    AllowNullCollections = true
                };

                 cfg.CreateMap<OptionsObject.Data, DataTables.SettingsTb>().ForMember(x => x.AutoIdSettings, opt => opt.Ignore());
                 cfg.CreateMap<UserDataObject, DataTables.InfoUsersTb>().ForMember(x => x.AutoIdInfoUsers, opt => opt.Ignore());
                 cfg.CreateMap<GenresObject.DataGenres, DataTables.GenresTb>().ForMember(x => x.AutoIdGenres, opt => opt.Ignore());
                 cfg.CreateMap<PricesObject.DataPrice, DataTables.PriceTb>().ForMember(x => x.AutoIdPrice, opt => opt.Ignore());
                 cfg.CreateMap<SoundDataObject, DataTables.SharedTb>().ForMember(x => x.AutoIdShared, opt => opt.Ignore());
                 cfg.CreateMap<SoundDataObject, DataTables.LatestDownloadsTb>().ForMember(x => x.AutoIdaLatestDownloads, opt => opt.Ignore());
                 
                Mapper.Initialize(cfg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}