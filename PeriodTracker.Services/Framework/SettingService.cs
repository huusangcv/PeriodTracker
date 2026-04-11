using Microsoft.EntityFrameworkCore;
using PeriodTracker.Entities;
using PeriodTracker.Entities.Framework;
using System.ComponentModel;
using System.Globalization;

namespace PeriodTracker.Services.Framework
{
    public interface ISettingService
    {
        T LoadSetting<T>() where T : new();
        Task SaveSetting<T>(T settings) where T : new();
    }
    public class SettingService(DataContext _dataContext) : ISettingService
    {
        public virtual T GetSettingByKey<T>(string key, T defaultValue = default!,
            bool loadSharedValueIfNotFound = false)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            var settings = GetAllSettingsCached();
            key = key.Trim().ToLowerInvariant();
            if (settings.ContainsKey(key))
            {
                var settingsByKey = settings[key];
                var setting = settingsByKey.FirstOrDefault();

                if (setting != null)
                    return To<T>(setting.Value);
            }

            return defaultValue;
        }
        public static object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }

        public static object To(object value, Type destinationType, CultureInfo culture)
        {
            if (value != null)
            {
                Type type = value.GetType();
                TypeConverter falconCustomTypeConverter = GetFalconCustomTypeConverter(destinationType);
                TypeConverter falconCustomTypeConverter2 = GetFalconCustomTypeConverter(type);
                if (falconCustomTypeConverter != null && falconCustomTypeConverter.CanConvertFrom(value.GetType()))
                {
                    return falconCustomTypeConverter.ConvertFrom(null, culture, value);
                }

                if (falconCustomTypeConverter2 != null && falconCustomTypeConverter2.CanConvertTo(destinationType))
                {
                    return falconCustomTypeConverter2.ConvertTo(null, culture, value, destinationType);
                }

                if (destinationType.IsEnum && value is int)
                {
                    return Enum.ToObject(destinationType, (int)value);
                }

                if (!destinationType.IsInstanceOfType(value))
                {
                    return Convert.ChangeType(value, destinationType, culture);
                }
            }

            return value;
        }

        public static T To<T>(object value)
        {
            return (T)To(value, typeof(T));
        }
        public virtual T LoadSetting<T>() where T : new()
        {
            var settings = Activator.CreateInstance<T>();

            foreach (var prop in typeof(T).GetProperties())
            {
                // get properties we can read and write to
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var key = typeof(T).Name + "." + prop.Name;
                //load by store
                var setting = GetSettingByKey<string>(key, loadSharedValueIfNotFound: true);
                if (setting == null)
                    continue;

                if (!GetFalconCustomTypeConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                if (!GetFalconCustomTypeConverter(prop.PropertyType).IsValid(setting))
                    continue;

                object value = GetFalconCustomTypeConverter(prop.PropertyType).ConvertFromInvariantString(setting);

                //set property
                prop.SetValue(settings, value, null);
            }

            return settings;
        }
        protected virtual IDictionary<string, IList<Setting>> GetAllSettingsCached()
        {
            var query = from s in _dataContext.Settings.AsNoTracking()
                        orderby s.Name
                        select s;
            var settings = query.ToList();
            var dictionary = new Dictionary<string, IList<Setting>>();
            foreach (var s in settings)
            {
                var resourceName = s.Name.ToLowerInvariant();
                var settingForCaching = new Setting
                {
                    Id = s.Id,
                    Name = s.Name,
                    Value = s.Value,

                };
                if (!dictionary.ContainsKey(resourceName))
                {
                    //first setting
                    dictionary.Add(resourceName, new List<Setting>
                        {
                            settingForCaching
                        });
                }
                else
                {
                    //already added
                    dictionary[resourceName].Add(settingForCaching);
                }
            }
            return dictionary;
        }

        public async Task<Setting> GetSettingById(int settingId)
        {
            if (settingId == 0)
                return null;
            return await _dataContext.Settings.FindAsync(settingId);
        }

        public virtual async Task InsertSetting(Setting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));
            _dataContext.Settings.Add(setting);
            await _dataContext.SaveChangesAsync();
        }

        public virtual async Task UpdateSetting(Setting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            var entity = await _dataContext.Settings.FirstOrDefaultAsync(d => d.Id == setting.Id);
            if (entity == null)
            {
                _dataContext.Settings.Add(setting);
            }
            else
            {
                // Fix: Update properties instead of replacing entity reference
                entity.Name = setting.Name;
                entity.Value = setting.Value;
                _dataContext.Settings.Update(entity);
            }
            await _dataContext.SaveChangesAsync();
        }

        public virtual async Task SetSetting<T>(string key, T value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = key.Trim();
            string valueStr = GetFalconCustomTypeConverter(typeof(T)).ConvertToInvariantString(value);

            // Fix: Get existing setting from database instead of cache
            var existingSetting = await _dataContext.Settings
                .FirstOrDefaultAsync(s => s.Name == key);

            if (existingSetting != null)
            {
                // Update existing setting
                existingSetting.Value = valueStr;
                await UpdateSetting(existingSetting);
            }
            else
            {
                // Insert new setting
                var setting = new Setting
                {
                    Name = key,
                    Value = valueStr,
                };
                await InsertSetting(setting);
            }
        }

        public virtual async Task SaveSetting<T>(T settings) where T : new()
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            foreach (var prop in typeof(T).GetProperties())
            {
                // get properties we can read and write to
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (!GetFalconCustomTypeConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                string key = typeof(T).Name + "." + prop.Name;

                // Get property value
                var value = prop.GetValue(settings, null);

                // Save the setting
                if (value != null)
                {
                    await SetSetting(key, value);
                }
                else
                {
                    await SetSetting(key, string.Empty);
                }
            }
        }

        public static TypeConverter GetFalconCustomTypeConverter(Type type)
        {
            if (type == typeof(List<int>))
            {
                return new GenericListTypeConverter<int>();
            }

            if (type == typeof(List<decimal>))
            {
                return new GenericListTypeConverter<decimal>();
            }

            if (type == typeof(List<string>))
            {
                return new GenericListTypeConverter<string>();
            }

            return TypeDescriptor.GetConverter(type);
        }
    }
}