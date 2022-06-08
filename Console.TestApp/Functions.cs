using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;
using viramesV4.Object.BusinessLogicLayer.Master;
using viramesV4.Object.BusinessLogicLayer.System;
using viramesV4.Object.Factory;
using viramesV4.Object.Interfaces;
namespace viramesV4.Object.HelperLayer
{
    public static class Functions
    {
        public static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Gönderilen tip bilgisine göre, verilebilinecek bir sonraki MaterialFiche kaydı için Number bilgisini oluşturup geri döndürmektedir.
        /// </summary>
        /// <param name="type">MaterialFiche içindeki Transaction Type bilgisi</param>
        /// <returns>String olarak, kaydedilebilinir bir sonraki Number bilgisini oluşturmaktadır.</returns>
        public static string CreateFicheNumber(TransactionType type)
        {
            try
            {
                string _string;
                string _stringReturn = string.Empty;
                if (virames<MaterialFiche>.Initialize(new List<MaterialFiche>()).Where(x => x.Type == type).GetList().Count > 0)
                {
                    _string = virames<MaterialFiche>.Initialize(new List<MaterialFiche>()).OrderBy(x => x.Number).Where(x => x.Type == type).GetList().Last().Number.ToUpper();
                    char[] _stringCharArray = _string.ToCharArray();
                    string[] _stringStringArray = new string[_string.Length];
                    for (int i = 0; i <= _stringCharArray.Length - 1; i++)
                    {
                        _stringStringArray[i] = _stringCharArray[i].ToString();
                    }
                    for (int i = _stringStringArray.Length - 1; i >= 0; i--)
                    {
                        if (_stringStringArray[i].ToString() != "9" && _stringStringArray[i].ToString() != "W")
                        {
                            _stringStringArray[i] = ((char)(Convert.ToInt32(Convert.ToChar(_stringStringArray[i])) + 1)).ToString();
                            break;
                        }
                        else if (_stringStringArray[i].ToString() == "9")
                        {
                            _stringStringArray[i] = "0";
                        }
                        else if (_stringStringArray[i].ToString() != "W")
                        {
                            _stringStringArray[i] = "A";
                        }
                    }
                    for (int i = 0; i <= _stringStringArray.Length - 1; i++)
                    {
                        _stringReturn += _stringStringArray[i].ToString();
                    }
                    return _stringReturn;
                }
                else
                {
                    return "0000000001";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// HTML formatında yazılmış metnin, HTML taglarından ayıklanmış versiyonunu döndürebilmek için çağırılan bir fonksiyondur.
        /// </summary>
        /// <param name="htmlString">Tagları ayıklanacak HTML string'i.</param>
        /// <returns>Parametre olarak gelen htmlString değişkeninin HTML Taglarından ayıklanmış halidir.</returns>
        public static string GetPlainTextFromHtml(string htmlString)
        {

            try
            {
                if (String.IsNullOrEmpty(htmlString))
                    return string.Empty;
                string htmlTagPattern = "<.*?>";
                var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                htmlString = regexCss.Replace(htmlString, string.Empty);
                htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
                htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                htmlString = htmlString.Replace("&nbsp;", string.Empty);
                StringWriter myWriter = new StringWriter();
                HttpUtility.HtmlDecode(htmlString, myWriter);
                htmlString = myWriter.ToString();
                return htmlString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// WF içerisinde oluşturulmuş Mesaj Şablonu içerisinde @PropertyName şeklinde belirtilen kısımları,
        /// ilgili property'nin value'su ile değiştirmek için oluşturulmuş bir fonksiyondur.
        /// </summary>
        /// <param name="message">Mesaj Şablonu</param>
        /// <param name="Entity">Nesne</param>
        /// <returns>Sisteme kaydedilecek ve sistem tarafından gönderilecek Message nesnesini kaydeder.</returns>
        internal static Message ReplaceWithProperty(Message message, IPocoMasters Entity)
        {
            try
            {
                Message returnMessage = message;
                string[] textList = message.MessagePreview.Split('\n', '\r', ' ', '@');
                int userId = (int)message.ReceiverID;
                User user = virames<User>.Initialize(new User()).Where(x => x.ID == message.ReceiverID).Take();
                foreach (var item in textList)
                {
                    if (item.StartsWith("User."))
                    {
                        string prop = item.Remove(0, 5);
                        prop = prop.Trim('\t', '\n', '\r');
                        //prop = prop.Substring(0, prop.IndexOf('<'));
                        message.MessageBody = message.MessageBody.Replace("@" + item.Substring(0, item.Length), user.GetType().GetProperty(prop).GetValue(user).ToString());
                    }
                    else
                    {
                        Type type = Entity.GetType().BaseType;
                        if (type.FullName == "System.Object")
                            type = Entity.GetType();
                        if (item.StartsWith(type.Name + "."))
                        {
                            string prop = item.Remove(0, item.IndexOf('.') + 1);
                            prop = prop.Trim('\t', '\n', '\r');
                            message.MessageBody = message.MessageBody.Replace("@" + item.Substring(0, item.Length), type.GetProperty(prop).GetValue(Entity).ToString());
                        }
                    }
                }
                return returnMessage;
            }
            catch (Exception ex)
            {
                throw new Exception("REPLACEWITHMSSG", ex);
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Kaydetmeden önce, ilgili kayıtla alakalı bir WF kaydının olup olmadığını kontrol etmektedir.
        /// Bir kayıt var ise, kayıt ile eşleştirilmiş Mesaj Şablonu ile mesaj kaydı gerçekleştirilir.
        /// </summary>
        /// <param name="entity">Kontrolü yapılacak kayıt.</param>
        internal static void BeforeSave(IPocoMasters entity)
        {
            try
            {
                User systemUser = virames<User>.Initialize(new User()).Where(x => x.Username == "System" && x.IsVisible == false).Take();
                if (systemUser == null)
                {
                    systemUser = new User();
                    systemUser.Username = "System";
                    systemUser.Password = Object.HelperLayer.Functions.MD5("abcdefghj123456");
                    systemUser.RoleID = virames<Role>.Initialize(new Role()).Where(x => x.UserType == UserType.Administrator).Take().ID;
                    systemUser.Name = "System";
                    systemUser.IsVisible = false;
                    systemUser.Save();
                }
                Type type = entity.GetType().BaseType;
                if (type.FullName.Contains("System.Object"))
                    type = entity.GetType();
                if (entity.ID > 0)
                {
                    List<WorkflowDesignLine> workflowDesignafterLines = virames<WorkflowDesignLine>.Initialize(new List<WorkflowDesignLine>()).Where(x => x.ModuleName == type.Name && x.OperationMoment == OperationMoment.AfterEdit && (x.DateControl != true || (x.DateControl == true && x.StartDate < DateTime.Now && x.EndDate > DateTime.Now))).GetList();
                    if (workflowDesignafterLines != null && workflowDesignafterLines.Count > 0)
                        Statics.AddtoEditedEntities(entity);
                    List<WorkflowDesignLine> workflowDesignLines = virames<WorkflowDesignLine>.Initialize(new List<WorkflowDesignLine>()).Where(x => x.ModuleName == type.Name && x.OperationMoment == OperationMoment.BeforeEdit && (x.DateControl != true || (x.DateControl == true && x.StartDate < DateTime.Now && x.EndDate > DateTime.Now))).GetList();
                    if (workflowDesignLines == null || workflowDesignLines.Count == 0)
                        return;
                    else
                    {
                        foreach (var item in workflowDesignLines)
                        {
                            if (item.RoleID != null && item.RoleID > 0)
                            {
                                List<User> users = virames<User>.Initialize(new List<User>()).Where(x => x.RoleID == item.RoleID && x.IsVisible == true).GetList();
                                foreach (var user in users)
                                {
                                    if (item.UserID != null && user.ID == item.UserID)
                                        continue;
                                    Message message = new Message()
                                    {
                                        Date = DateTime.Now,
                                        GUID = Guid.NewGuid().ToString(),
                                        MessageLocationType = MessageLocationType.Received,
                                        OwnerID = user.ID,
                                        ReceiverID = user.ID,
                                        SenderID = systemUser.ID,
                                        Subject = item.WorkflowDesign.Code,
                                        Status = MessageStatus.UnShowed,
                                        MessageBody = item.WorkflowDesignMessageTemplate.MessageTemplate
                                    };
                                    message = ReplaceWithProperty(message, entity);
                                    message.Save();
                                }
                            }
                            if (item.UserID != null && item.UserID > 0)
                            {
                                Message message = new Message()
                                {
                                    Date = DateTime.Now,
                                    GUID = Guid.NewGuid().ToString(),
                                    MessageLocationType = MessageLocationType.Received,
                                    OwnerID = item.UserID,
                                    ReceiverID = item.UserID,
                                    SenderID = systemUser.ID,
                                    Subject = item.WorkflowDesign.Code,
                                    Status = MessageStatus.UnShowed,
                                    MessageBody = item.WorkflowDesignMessageTemplate.MessageTemplate
                                };
                                message = ReplaceWithProperty(message, entity);
                                message.Save();
                            }
                        }
                    }
                }
                else
                {
                    List<WorkflowDesignLine> workflowDesignBeforeLines = virames<WorkflowDesignLine>.Initialize(new List<WorkflowDesignLine>()).Where(x => x.ModuleName == type.Name && x.OperationMoment == OperationMoment.AfterSave && (x.DateControl != true || (x.DateControl == true && x.StartDate < DateTime.Now && x.EndDate > DateTime.Now))).GetList();
                    if (workflowDesignBeforeLines != null && workflowDesignBeforeLines.Count > 0)
                        Statics.AddtoSavedEntities(entity);
                    List<WorkflowDesignLine> workflowDesignLines = virames<WorkflowDesignLine>.Initialize(new List<WorkflowDesignLine>()).Where(x => x.ModuleName == type.Name && x.OperationMoment == OperationMoment.BeforeSave && (x.DateControl != true || (x.DateControl == true && x.StartDate < DateTime.Now && x.EndDate > DateTime.Now))).GetList();
                    if (workflowDesignLines == null || workflowDesignLines.Count == 0)
                        return;
                    else
                    {
                        foreach (var item in workflowDesignLines)
                        {
                            if (item.RoleID != null && item.RoleID > 0)
                            {
                                List<User> users = virames<User>.Initialize(new List<User>()).Where(x => x.RoleID == item.RoleID && x.IsVisible == true).GetList();
                                foreach (var user in users)
                                {
                                    if (item.UserID != null && user.ID == item.UserID)
                                        continue;
                                    Message message = new Message()
                                    {
                                        Date = DateTime.Now,
                                        GUID = Guid.NewGuid().ToString(),
                                        MessageLocationType = MessageLocationType.Received,
                                        OwnerID = user.ID,
                                        ReceiverID = user.ID,
                                        SenderID = systemUser.ID,
                                        Subject = item.WorkflowDesign.Code,
                                        Status = MessageStatus.UnShowed,
                                        MessageBody = item.WorkflowDesignMessageTemplate.MessageTemplate
                                    };
                                    message = ReplaceWithProperty(message, entity);
                                    message.Save();
                                }
                            }
                            if (item.UserID != null && item.UserID > 0)
                            {
                                Message message = new Message()
                                {
                                    Date = DateTime.Now,
                                    GUID = Guid.NewGuid().ToString(),
                                    MessageLocationType = MessageLocationType.Received,
                                    OwnerID = item.UserID,
                                    ReceiverID = item.UserID,
                                    Subject = item.WorkflowDesign.Code,
                                    SenderID = systemUser.ID,
                                    Status = MessageStatus.UnShowed,
                                    MessageBody = item.WorkflowDesignMessageTemplate.MessageTemplate
                                };
                                message = ReplaceWithProperty(message, entity);
                                message.Save();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("WFFUNCTION", ex);
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Kaydı gerçekleşen nesneleri, Statics içerisindeki bir collection içerisinden alır ve Kaydettikten sonra tetiklenecek bir WF kaydı olup olmadığına bakar.\n
        /// Var ise, seçilen mesaj şablonu ile sistem belirtilen kullanıcı ve ya rollere mesaj gönderecektir.
        /// </summary>
        internal static void AfterSave()
        {
            try
            {
                User systemUser = virames<User>.Initialize(new User()).Where(x => x.Username == "System" && x.IsVisible == false).Take();
                if (systemUser == null)
                {
                    systemUser = new User();
                    systemUser.Username = "System";
                    systemUser.Password = Object.HelperLayer.Functions.MD5("abcdefghj123456");
                    systemUser.RoleID = virames<Role>.Initialize(new Role()).Where(x => x.UserType == UserType.Administrator).Take().ID;
                    systemUser.Name = "System";
                    systemUser.IsVisible = false;
                    systemUser.Save();
                }
                foreach (var enty in Statics.GetSavedEntities())
                {
                    Type type = enty.GetType().BaseType;
                    if (type.FullName == "System.Object")
                        type = enty.GetType();
                    List<WorkflowDesignLine> workflowDesignBeforeLines = virames<WorkflowDesignLine>.Initialize(new List<WorkflowDesignLine>()).Where(x => x.ModuleName == type.Name && x.OperationMoment == OperationMoment.AfterSave && (x.DateControl != true || (x.DateControl == true && x.StartDate < DateTime.Now && x.EndDate > DateTime.Now))).GetList();
                    if (workflowDesignBeforeLines == null || workflowDesignBeforeLines.Count == 0)
                        return;
                    else
                    {

                        foreach (var item in workflowDesignBeforeLines)
                        {
                            if (item.RoleID != null && item.RoleID > 0)
                            {
                                List<User> users = virames<User>.Initialize(new List<User>()).Where(x => x.RoleID == item.RoleID).GetList();
                                foreach (var user in users)
                                {
                                    if (item.UserID != null && user.ID == item.UserID)
                                        continue;
                                    Message message = new Message()
                                    {
                                        Date = DateTime.Now,
                                        GUID = Guid.NewGuid().ToString(),
                                        MessageLocationType = MessageLocationType.Received,
                                        OwnerID = user.ID,
                                        ReceiverID = user.ID,
                                        Subject = item.WorkflowDesign.Code,
                                        SenderID = systemUser.ID,
                                        Status = MessageStatus.UnShowed,
                                        MessageBody = item.WorkflowDesignMessageTemplate.MessageTemplate
                                    };
                                    message = ReplaceWithProperty(message, enty);
                                    message.Save();
                                }
                            }
                            if (item.UserID != null && item.UserID > 0)
                            {
                                Message message = new Message()
                                {
                                    Date = DateTime.Now,
                                    GUID = Guid.NewGuid().ToString(),
                                    MessageLocationType = MessageLocationType.Received,
                                    OwnerID = item.UserID,
                                    ReceiverID = item.UserID,
                                    Subject = item.WorkflowDesign.Code,
                                    SenderID = systemUser.ID,
                                    Status = MessageStatus.UnShowed,
                                    MessageBody = item.WorkflowDesignMessageTemplate.MessageTemplate
                                };
                                message = ReplaceWithProperty(message, enty);
                                message.Save();
                            }
                        }
                    }
                }
                Statics.SetSavedEntities(new List<IPocoMasters>());
                foreach (var enty in Statics.GetEditedEntities())
                {
                    Type type = enty.GetType().BaseType;
                    if (type.FullName == "System.Object")
                        type = enty.GetType();
                    List<WorkflowDesignLine> workflowDesignBeforeLines = virames<WorkflowDesignLine>.Initialize(new List<WorkflowDesignLine>()).Where(x => x.ModuleName == type.Name && x.OperationMoment == OperationMoment.AfterEdit && (x.DateControl != true || (x.DateControl == true && x.StartDate < DateTime.Now && x.EndDate > DateTime.Now))).GetList();
                    if (workflowDesignBeforeLines == null || workflowDesignBeforeLines.Count == 0)
                        return;
                    else
                    {
                        foreach (var item in workflowDesignBeforeLines)
                        {
                            if (item.RoleID != null && item.RoleID > 0)
                            {
                                List<User> users = virames<User>.Initialize(new List<User>()).Where(x => x.RoleID == item.RoleID).GetList();
                                foreach (var user in users)
                                {
                                    if (item.UserID != null && user.ID == item.UserID)
                                        continue;
                                    Message message = new Message()
                                    {
                                        Date = DateTime.Now,
                                        GUID = Guid.NewGuid().ToString(),
                                        MessageLocationType = MessageLocationType.Received,
                                        OwnerID = user.ID,
                                        ReceiverID = user.ID,
                                        Subject = item.WorkflowDesign.Code,
                                        SenderID = systemUser.ID,
                                        Status = MessageStatus.UnShowed,
                                        MessageBody = item.WorkflowDesignMessageTemplate.MessageTemplate
                                    };
                                    message = ReplaceWithProperty(message, enty);
                                    message.Save();
                                }
                            }
                            if (item.UserID != null && item.UserID > 0)
                            {
                                Message message = new Message()
                                {
                                    Date = DateTime.Now,
                                    GUID = Guid.NewGuid().ToString(),
                                    MessageLocationType = MessageLocationType.Received,
                                    OwnerID = item.UserID,
                                    ReceiverID = item.UserID,
                                    Subject = item.WorkflowDesign.Code,
                                    SenderID = systemUser.ID,
                                    Status = MessageStatus.UnShowed,
                                    MessageBody = item.WorkflowDesignMessageTemplate.MessageTemplate
                                };
                                message = ReplaceWithProperty(message, enty);
                                message.Save();
                            }
                        }
                    }
                }
                Statics.SetEditedEntities(new List<IPocoMasters>());
            }
            catch (Exception ex)
            {
                throw new Exception("WFFUNCTION", ex);
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// viramesV4.Object.BusinessLogicLayer.Master isim uzayında var olan type bilgilerini bir List halinde geri döndürür.
        /// </summary>
        /// <returns>Master altındaki sınıfların tip listesi</returns>
        public static List<Type> GetTypesInNameSpaceWithNames()
        {
            try
            {
                return GetTypesInNamespace(Assembly.GetExecutingAssembly(), "viramesV4.Object.BusinessLogicLayer.Master").Where(x => !x.Name.Contains("<>")).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Belirli bir Assambly içerisindeki Belirli bir isim uzayının içindeki tipleri bir dizi olarak döndürür.
        /// </summary>
        /// <param name="assembly">Tip bilglierinin kontrolünün yapılacağı Assembly.</param>
        /// <param name="nameSpace">Tip bilgilerinin kontrol edileceği isim uzayı.</param>
        /// <returns>Tespit edilen tiplerin dizi tipindeki listesi.</returns>
        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            try
            {
                return
                 assembly.GetTypes()
                  .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                  .ToArray();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Master, System, Factory, HelperLayer, FClasses vs isim uzaylarının altındaki classların fonksiyonlarında gerçekleşen Exceptionlara, otomatik kod oluşturur.\n
        /// Bu kod, ilgili sınıfın sıra numarası, fonksiyon tipi ve Master isim uzayında olup olmadığına göre oluşmaktadır.\n
        /// Master altında ise "1X" ile, değilse "0X" ile başlar.
        /// </summary>
        /// <param name="functionType">Fonksiyon tipini belirtir. Tiplere göre OT, DS, DL gibi iştem bilgisi exception kodunda belirtilir.</param>
        /// <param name="obj">Exception anındaki mevcut nesnedir.</param>
        /// <param name="IsMaster">Master klasörünün altında bulunup bulunmadığını belirtir.</param>
        /// <returns>Otomatik olarak oluşturulan bir ExceptionCode döndürür.</returns>
        public static string GetExceptionCode(FunctionType functionType, object obj, bool IsMaster = true)
        {
            try
            {
                string className = obj.GetType().BaseType.Name.Equals("Object") ? obj.GetType().Name : obj.GetType().BaseType.Name;
                string returnString = string.Empty;
                returnString += IsMaster ? "1X" : "0X";
                List<Type> typelist;
                if (IsMaster)
                    typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "viramesV4.Object.BusinessLogicLayer.Master").Where(x => !x.Name.Contains("<>")).OrderBy(x => x.Name).ToList();
                else
                    typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "viramesV4.Object.BusinessLogicLayer.System").Where(x => !x.Name.Contains("<>")).OrderBy(x => x.Name).ToList();
                returnString += ((typelist.IndexOf(typelist.Where(x => x.Name == className).FirstOrDefault())) + 1).ToString().PadLeft(3, '0') + functionType.ToString();
                return returnString;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Parametre olarak gönderilen nesneyi, ilk önce clone yapar ve daha sonra, ID fieldlarını 0 yapar, virtual alanlar nullenir ve object tipinde geri döndürülür.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>obj Nesnesinin kopyalama işlemi yapılmış halini döndürür.</returns>
        public static object CopyObject(object obj)
        {
            try
            {
                object clonnedobject = CloneObject(obj, false);
                foreach (PropertyInfo p in clonnedobject.GetType().GetProperties())
                {
                    if (p.PropertyType.FullName.Contains("System.Collections.Generic.List") && p.CanWrite)
                    {
                        var line = (System.Collections.IList)obj.GetType().GetProperty(p.Name).GetValue(obj);
                        if (line != null && line.Count > 0)
                        {
                            Type itemType = line[0].GetType().BaseType;
                            if (itemType.FullName == "System.Object")
                                itemType = line[0].GetType();
                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(itemType);
                            var list = (IList)Activator.CreateInstance(constructedListType);
                            foreach (object item in line)
                            {
                                list.Add(CopyObject(item));
                            }
                            Type type = clonnedobject.GetType().GetProperty(p.Name).GetValue(obj).GetType();
                            clonnedobject.GetType().GetProperty(p.Name).SetValue(clonnedobject, Convert.ChangeType(list, type));
                        }
                        else
                        {
                            Type emptyListType = obj.GetType().GetProperty(p.Name).PropertyType;
                            var list = (IList)Activator.CreateInstance(emptyListType);
                            clonnedobject.GetType().GetProperty(p.Name).SetValue(clonnedobject, Convert.ChangeType(list, emptyListType));
                        }
                    }
                    else if (p.Name != "ID" && p.GetMethod.IsVirtual && p.CanWrite)
                        clonnedobject.GetType().GetProperty(p.Name).SetValue(clonnedobject, null, null);
                    else if (p.Name == "ID")
                        clonnedobject.GetType().GetProperty(p.Name).SetValue(clonnedobject, 0);

                }
                return clonnedobject;
            }
            catch (Exception ex)
            {
                ex = new Exception(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// İlgili nesneyi, içerisindeki ilk virtualları da dahil ederek, Entity Change Tracer'dan kurtarmak için kullandığımız fonksiyondur.
        /// İlk seviyede Listeler de kopyalanır ancak ilk liste ve liste içindeki nesnelerden başlayarak karşılıklarını null yapar ve daha derine inmez.
        /// ID bilgisi değiştirilmez, referanslar bozulmaz.
        /// </summary>
        /// <param name="obj">Clone işlemini gerçekleştireceğimiz nesne</param>
        /// <param name="downmovement">İlk katmanda true olarak ayarlarız, böylece ilk nesnenin listlerini de virtual özellikleri ile birlikte klonlar fakat recursive esnasında false döndürülür ve artık alt virtual nesneler KLONLANMAZ.</param>
        /// <returns>Klonlanan nesne döndürülür.</returns>
        public static object CloneObject(object obj, bool downmovement = true)
        {
            try
            {
                Type objectType = obj.GetType().BaseType;
                if (objectType.FullName == "System.Object")
                    objectType = obj.GetType();
                object clonedObject = Activator.CreateInstance(objectType);
                foreach (PropertyInfo p in objectType.GetProperties())
                {
                    if (p.PropertyType.FullName.Contains("System.Collections.Generic.List") && p.CanWrite)
                    {
                        var line = (System.Collections.IList)obj.GetType().GetProperty(p.Name).GetValue(obj);
                        if (line != null && line.Count > 0)
                        {
                            Type itemType = line[0].GetType().BaseType;
                            if (itemType.FullName == "System.Object")
                                itemType = line[0].GetType();
                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(itemType);
                            var list = (IList)Activator.CreateInstance(constructedListType);
                            foreach (object item in line)
                            {
                                list.Add(CloneObject(item, false));
                            }
                            Type type = clonedObject.GetType().GetProperty(p.Name).GetValue(obj).GetType();
                            clonedObject.GetType().GetProperty(p.Name).SetValue(clonedObject, Convert.ChangeType(list, type));
                        }
                        else
                        {
                            Type emptyListType = obj.GetType().GetProperty(p.Name).PropertyType;
                            var list = (IList)Activator.CreateInstance(emptyListType);
                            clonedObject.GetType().GetProperty(p.Name).SetValue(clonedObject, Convert.ChangeType(list, emptyListType));
                        }
                    }
                    else if (p.Name != "ID" && !p.Name.Contains("CreatedBy") && !p.Name.Contains("CreatedDate") && !p.Name.Contains("ModifiedBy") && !p.Name.Contains("ModifiedDate") && p.PropertyType != typeof(IEnumerable).GetType() && p.GetMethod.IsVirtual && p.GetValue(obj) != null && p.CanWrite)
                    {
                        if (downmovement && !(p.PropertyType.FullName.Contains("System.ComponentModel.BindingList")))
                        {
                            var x = CloneObject(obj.GetType().GetProperty(p.Name).GetValue(obj));
                            clonedObject.GetType().GetProperty(p.Name).SetValue(clonedObject, x);
                        }
                    }
                    else if (p.CanWrite)
                        clonedObject.GetType().GetProperty(p.Name).SetValue(clonedObject, obj.GetType().GetProperty(p.Name).GetValue(obj));
                }
                return clonedObject;
            }
            catch (Exception ex)
            {
                throw new Exception("Clone Error!", ex);
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// İki nesnenin eşit olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="aObject">Karşılaştırılan iki nesneden ilki.</param>
        /// <param name="bObject">Karşılaştırılan iki nesneden ikincisi.</param>
        /// <returns>Karşılaştırılan nesneler aynı ise true, değil ise false dönecektir.</returns>
        public static bool Equals(object aObject, object bObject)
        {
            try
            {
                bool value = true;
                foreach (var item in aObject.GetType().GetProperties().Where(x => x.CanWrite == true && x.GetGetMethod().IsVirtual == false))
                {
                    var aValue = item.GetValue(aObject) == null ? " " : item.GetValue(aObject);
                    var bValue = item.GetValue(bObject) == null ? " " : item.GetValue(bObject);
                    if (!aValue.Equals(bValue))
                    {
                        value = false;
                        break;
                    }
                }
                return value;
            }
            catch (Exception ex)
            {
                ex = new Exception(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// İlgili entity'nin içerisindeki _entityWrapper üzerinden, nesnenin bağlı bulunduğu entity context verisine ulaşmak için kullanılır.
        /// </summary>
        /// <param name="entity">Herhangi bir entity context'e bağlı bulunan ve entity contextine erişilmesi istenen nesne.</param>
        /// <returns>ilgili nesnenin bağlı bulunduğu viramesMasterContext tipindeki entity contexti döndürür.</returns>
        public static viramesMasterContext GetDbContextFromEntity(object entity)
        {
            var object_context = GetObjectContextFromEntity(entity);

            if (object_context == null)
                return null;

            return new viramesMasterContext(object_context, dbContextOwnsObjectContext: false);
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// İlgili entity'nin içerisindeki _entityWrapper üzerinden, nesnenin bağlı bulunduğu object context verisine ulaşmak için kullanılır.
        /// </summary>
        /// <param name="entity">Herhangi bir object context'e bağlı bulunan ve contextine erişilmesi istenen nesne.</param>
        /// <returns>ilgili nesnenin bağlı bulunduğu ObjectContext tipindeki contexti döndürür.</returns>
        private static ObjectContext GetObjectContextFromEntity(object entity)
        {
            try
            {
                var field = entity.GetType().GetField("_entityWrapper");

                if (field == null)
                    return null;

                var wrapper = field.GetValue(entity);
                var property = wrapper.GetType().GetProperty("Context");
                var context = (ObjectContext)property.GetValue(wrapper, null);

                return context;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Gönderilen Byte Array tipindeki nesnenin içindeki gereksiz ve bir anlam ifade etmeyen elemanları temizlemek amacı ile kullanılır.
        /// </summary>
        /// <param name="targetArray">Boş alanları temizlenmesi istenen array</param>
        /// <returns>Boş alanları temizlenmiş arraym</returns>
        public static byte[] TrimArray(byte[] targetArray)
        {
            try
            {
                IEnumerator enum1 = targetArray.GetEnumerator();
                byte[] bufferArray = new byte[targetArray.Length];
                int i = 0;
                int j = 0;
                while (enum1.MoveNext())
                {
                    if (enum1.Current.ToString().Equals("0"))
                    {
                        j++;
                        continue;
                    }
                    i++;
                    bufferArray[i - 1] = targetArray[i + j - 1];
                }
                byte[] bufferTrim = new byte[i];
                for (int a = 0; a < i; a++)
                    bufferTrim[a] = bufferArray[a];
                return bufferTrim;
            }
            catch (Exception ex)
            {
                ex = new Exception(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Nesneyi XML formatına dönüştürür.
        /// </summary>
        /// <typeparam name="T">XML dönüşümü yaparken, dönüşümün referans alınacağı T tipi.</typeparam>
        /// <param name="toSerialize">XML'e dönüştürülecek nesne.</param>
        /// <returns>XML formatındaki string döner.</returns>
        public static string SerializeObject<T>(this T toSerialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, toSerialize);
                    return textWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Gönderilen barkod stringinin EAN13 kontrol rakamını oluşturur.
        /// </summary>
        /// <param name="barcode">Kontrol karakterinin hesaplanmasını istediğimiz barkod string'i</param>
        /// <returns>EAN13 kontrol rakamı döndürür.</returns>
        public static int CalculateEAN13ControlKey(string barcode)
        {
            if (barcode.Length != 12)
                throw new Exception("EAN13 Barcode length failure!");
            try
            {
                int factor = 3;
                int sum = 0;
                for (int index = barcode.Length - 1; index >= 0; index--)
                {
                    sum = sum + Convert.ToInt32(barcode.Substring(index, 1)) * factor;
                    factor = 4 - factor;
                }
                int cc = ((1000 - sum) % 10);
                return cc;
            }
            catch (Exception ex)
            {
                ex = new Exception("Calculate failure for EAN13 barcode type!", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// XML'den T tipine nesneyi geri dönüştürür.
        /// </summary>
        /// <typeparam name="T">Dönüşümün yapılacağı anda referans alınacak T tipi</typeparam>
        /// <param name="objectData">Dönüşümün yapılacağı XML dokümanı</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(this string objectData)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                T result;

                using (TextReader reader = new StringReader(objectData))
                {
                    result = (T)serializer.Deserialize(reader);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Şifrelemelerimizde hash key olarak kullanılır.
        /// </summary>
        internal static string key = "DizaynBirYazilimKurulusudurve2001YilindaKurulmustur";
        private static object _obj;
        private static PropertyInfo pinfo;

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Makinenin ana kartı üzerindeki seri numarası bilgisini döndürür.
        /// Terminal kaydederken Key bilgisi olarak tutulmaktadır.
        /// </summary>
        /// <returns>Makinenin üzerindeki ana kartın seri numarasını döndürür</returns>
        public static string GetMachineID()
        {
            try
            {
                string mbInfo = String.Empty;
                ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
                scope.Connect();
                ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

                foreach (PropertyData propData in wmiClass.Properties)
                {
                    if (propData.Name == "SerialNumber")
                        mbInfo = String.Format("{0}", Convert.ToString(propData.Value));
                }

                return mbInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// toEncrypt stringi ile gönderilen metni, şifreler ve şifreli halini geri döndürür.\n
        /// Eğer useHasing = true ise yukarıdaki key bilgisi ise, şifrelerken bir hash işlemi uygular ve böylece bu keye sahip olmadan geri dönüşüm gerçekleşemez.
        /// </summary>
        /// <param name="toEncrypt">Şifrelenecek Metin</param>
        /// <param name="useHashing">Key'e göre şifreleme yapılıp yapılmayacağını belirten değişken</param>
        /// <returns>Şifrelenmiş metin döndürür.</returns>
        public static string Crypto(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Şifreli metni çözüp tekrar şifrelenmeden önceki haline geri getirmek için kullanılır.\n
        /// Şifreli metinle birlikte, şifrelenirken Key değişkenine göre hashlenip hashlenmediğini belirleyen userHasing değişkeni kullanılır. 
        /// </summary>
        /// <param name="cipherString">Şifreli metin</param>
        /// <param name="useHashing">Şifrenin tekrar Key parametresine göre çözülüp çözülmeyeceğini belirleyen parametre.</param>
        /// <returns>Şifresi çözülmüş metin.</returns>
        public static string Encrypto(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Metni MD5 formatına göre şifreler. MD5'e göre şifrelenen metin, geri dönüştürülemez.
        /// </summary>
        /// <param name="text">Şifrelenmek istenen metin</param>
        /// <returns>Şifrelenmiş metin</returns>
        public static string MD5(string text)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] btr = Encoding.UTF8.GetBytes(text);
            btr = md5.ComputeHash(btr);
            StringBuilder sb = new StringBuilder();
            foreach (byte bt in btr)
            {
                sb.Append(bt.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Byte array haline dönüştürülen dokümanı şifrelemek için kullanılır.
        /// Şifreleme esnasında, userHashing değişkeni, true ya da false ise şifreleme esnasında Key değişkenine göre şifreleme yapılır ya da yapılmaz.
        /// </summary>
        /// <param name="toEncrypt">Şifrelenecek dokümanın byte array hali</param>
        /// <param name="useHashing">Şifreleme anında Key değişkenini şifrelemenin içerisinde dahil edip edilmeyeceğini belirleyen parametre</param>
        /// <returns>Şifrelenmiş doküman</returns>
        public static byte[] CryptoDocument(byte[] toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = toEncrypt;
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return resultArray;
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Şifrelenmiş dokümanın şifresini çözerek orjinal haline dönüştürmek için kullanılır. 
        /// </summary>
        /// <param name="cipherString">Şifreli Doküman</param>
        /// <param name="useHashing">Key'e göre hashleme yapılıp yapılmayacağını karar veren parametre</param>
        /// <returns>Şifresi çözülmüş orjinal doküman</returns>
        public static byte[] EncryptoDocument(byte[] cipherString, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = cipherString;
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return resultArray;
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Gönderilen HTML'in stil taglarını temizlemek için kullanılır.
        /// </summary>
        /// <param name="htmlstring">HTML metin</param>
        /// <returns>Stil tagları temizlenen metin</returns>
        public static string ClearStyleTags(string htmlstring)
        {
            try
            {
                string returnStr = string.Empty;
                int indexOfOpen = htmlstring.IndexOf("<style");
                int indexOfClose = htmlstring.IndexOf("</style>", indexOfOpen + 1);
                returnStr = htmlstring.Substring(0, indexOfOpen) + htmlstring.Substring(indexOfClose + 8);
                returnStr = returnStr.Replace("              ", string.Empty);
                return returnStr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Gönderilen malzeme kartına ait , belirtilen miktar ve MovementType ile SerialLotNumber ve SerialLotTransaction kayıtları oluşturur.
        /// </summary>
        /// <param name="material">Oluşturulacak seri numaralarının malzeme bilgisi, lot büyüklüğü gibi bilgilerini barındırır ve bu bilgilere göre numara kayıtları oluşturulur.</param>
        /// <param name="createamount">Oluşturulacak miktar bilgisi verilir.</param>
        /// <param name="serialLotTransactionMovementType">Seri numaralarının toplamlarını belirlerken esas alınan MovementType bilgisi</param>
        /// <returns>Oluşturulan Seri Lot Numaralarına ait hareket bilgilerini döndürür.</returns>
        public static List<SerialLotTransaction> CreateNumbersAndTransactions(Material material, double createamount, SerialLotTransactionMovementType serialLotTransactionMovementType)
        {
            try
            {
                List<SerialLotTransaction> transactions = new List<SerialLotTransaction>();
                double amount = 0;
                double remaining = 0;
                float oldLotCount = material.LotCount;
                float LotCount = material.LotCount;
                if (material.LotCount == 0 && material.TraceType == TraceType.PartNumber) LotCount = Convert.ToSingle(createamount);
                else if (material.TraceType == TraceType.SerialNumber)
                    LotCount = 1; ;
                if (material.TraceType == TraceType.PartNumber)
                {
                    amount = Convert.ToDouble(Math.Floor(createamount / LotCount));
                    remaining = createamount % material.LotCount;
                }
                else
                    amount = Convert.ToDouble(createamount);
                string fixedValue = string.Empty;
                string incrementalValue = "000000000";
                string incrementalLastValue = "0";
                string queryString = "";
                string resultString = "";
                List<ThreeDimension> templatepattern = new List<ThreeDimension>();
                for (int i = 0; i < amount; i++)
                {
                    resultString = "";
                    fixedValue = string.Empty;
                    incrementalValue = "000000000";
                    incrementalLastValue = "0";
                    queryString = "";
                    if (material.SerialTemplate == null)
                        material.SerialTemplate = virames<SerialTemplate>.Initialize(new SerialTemplate()).Where(x => x.Default == true).Take();
                    foreach (var item in material.SerialTemplate.Lines)
                    {
                        if (item.TemplateLineType == TemplateLineType.Fixed)
                        {
                            switch (item.Property)
                            {
                                case Property.Uncertain:
                                    templatepattern.Add(new ThreeDimension()
                                    {
                                        Key = item.ID.ToString(),
                                        DefaultValue = item.Start.ToString(),
                                        Fixed = true,
                                        Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                        Lenght = item.Start.ToString().Length,
                                        StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                    });
                                    resultString += item.Start.ToString();
                                    fixedValue += item.Start.ToString();
                                    queryString += item.Start.ToString();
                                    break;
                                case Property.MaterialID:
                                    templatepattern.Add(new ThreeDimension()
                                    {
                                        Key = item.ID.ToString(),
                                        DefaultValue = material.ID.ToString(),
                                        Fixed = true,
                                        Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                        Lenght = material.ID.ToString().Length,
                                        StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                    });
                                    resultString += material.ID.ToString();
                                    fixedValue += material.ID.ToString();
                                    queryString += material.ID.ToString();
                                    break;
                                case Property.MaterialType:
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = material.MaterialTypeID.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = material.MaterialTypeID.ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += material.MaterialTypeID.ToString();
                                        fixedValue += material.MaterialTypeID.ToString();
                                        queryString += material.MaterialTypeID.ToString();
                                    }
                                    break;
                                case Property.MaterialTraceType:
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = ((int)material.TraceType).ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ((int)material.TraceType).ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += ((int)material.TraceType).ToString();
                                        fixedValue += ((int)material.TraceType).ToString();
                                        queryString += ((int)material.TraceType).ToString();
                                    }
                                    break;
                                case Property.WorkareaID:
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = Statics.GetWorkarea().ID.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = Statics.GetWorkarea().ID.ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += Statics.GetWorkarea().ID.ToString();
                                        fixedValue += Statics.GetWorkarea().ID.ToString();
                                        queryString += Statics.GetWorkarea().ID.ToString();
                                    }
                                    break;
                                case Property.Second:
                                    if (DateTime.Now.Second < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Second.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Second.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Second.ToString();
                                        fixedValue += "0" + DateTime.Now.Second.ToString();
                                        queryString += "0" + DateTime.Now.Second.ToString();
                                    }
                                    else
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Second.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Second.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Second.ToString();
                                        fixedValue += DateTime.Now.Second.ToString();
                                        queryString += DateTime.Now.Second.ToString();
                                    }
                                    break;
                                case Property.Minute:
                                    if (DateTime.Now.Minute < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Minute.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Minute.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Minute.ToString();
                                        fixedValue += "0" + DateTime.Now.Minute.ToString();
                                        queryString += "0" + DateTime.Now.Minute.ToString();
                                    }
                                    else
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Minute.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Minute.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Minute.ToString();
                                        fixedValue += DateTime.Now.Minute.ToString();
                                        queryString += DateTime.Now.Minute.ToString();
                                    }
                                    break;
                                case Property.Hour:
                                    if (DateTime.Now.Hour < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Hour.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Hour.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Hour.ToString();
                                        fixedValue += "0" + DateTime.Now.Hour.ToString();
                                        queryString += "0" + DateTime.Now.Hour.ToString();
                                    }
                                    else
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Hour.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Hour.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Hour.ToString();
                                        fixedValue += DateTime.Now.Hour.ToString();
                                        queryString += DateTime.Now.Hour.ToString();
                                    }
                                    break;
                                case Property.Day:
                                    if (DateTime.Now.Day < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Day.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Day.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Day.ToString();
                                        fixedValue += "0" + DateTime.Now.Day.ToString();
                                        queryString += "0" + DateTime.Now.Day.ToString();
                                    }
                                    else
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Day.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Day.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Day.ToString();
                                        fixedValue += DateTime.Now.Day.ToString();
                                        queryString += DateTime.Now.Day.ToString();
                                    }
                                    break;
                                case Property.Mounth:
                                    if (DateTime.Now.Month < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Month.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Month.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Month.ToString();
                                        fixedValue += "0" + DateTime.Now.Month.ToString();
                                        queryString += "0" + DateTime.Now.Month.ToString();
                                    }
                                    else
                                    {
                                        resultString += DateTime.Now.Month.ToString();
                                        fixedValue += DateTime.Now.Month.ToString();
                                        queryString += DateTime.Now.Month.ToString();
                                    }
                                    break;
                                case Property.Year:
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Year.ToString().Substring(2, 2),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Year.ToString().Substring(2, 2)).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Year.ToString().Substring(2, 2);
                                        fixedValue += DateTime.Now.Year.ToString().Substring(2, 2);
                                        queryString += DateTime.Now.Year.ToString().Substring(2, 2);
                                    }
                                    break;
                                case Property.MaterialCode:
                                    templatepattern.Add(new ThreeDimension()
                                    {
                                        Key = item.ID.ToString(),
                                        DefaultValue = material.Code.Trim(),
                                        Fixed = true,
                                        Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                        Lenght = (material.Code.Trim()).Length,
                                        StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                    });
                                    resultString += material.Code.Trim();
                                    fixedValue += material.Code.Trim();
                                    queryString += material.Code.Trim();
                                    break;
                                case Property.MainUnitQuantity:
                                    templatepattern.Add(new ThreeDimension()
                                    {
                                        Key = item.ID.ToString(),
                                        DefaultValue = item.Start.ToString() + item.End.ToString(),
                                        Fixed = true,
                                        Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                        Lenght = (item.Start.ToString() + item.End.ToString()).Length,
                                        StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                    });
                                    resultString += item.Start.ToString() + item.End.ToString();
                                    fixedValue += item.Start.ToString() + item.End.ToString();
                                    queryString += item.Start.ToString().Replace('n', '_') + new String('_', item.End.ToString().Length);
                                    break;
                            }
                        }
                        else
                        {
                            templatepattern.Add(new ThreeDimension()
                            {
                                Key = item.ID.ToString(),
                                DefaultValue = item.Start.ToString(),
                                Fixed = false,
                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                Lenght = (item.Start.ToString()).Length,
                                StartIndex = fixedValue.Length == 0 ? 0 : fixedValue.Length
                            });
                            resultString += item.Start.ToString();
                            incrementalValue = item.Start.ToString();
                            incrementalLastValue = item.End.ToString();
                            queryString += new String('_', item.Start.ToString().Length);
                        }
                    }
                    SerialLotNumber serials = new SerialLotNumber();
                    using (var context = new viramesMasterContext())
                    {
                        serials = context.SerialLotNumbers.SqlQuery("select ID as ID, MATERIALID as MaterialID, TRACETYPE as TraceType, SERIALLOTNO as SerialLotNo, SIZE as Size, CREATEDBY as CreatedBy, CREATEDDATE as CreatedDate, MODIFIEDBY as ModifiedBy, MODIFIEDDATE as ModifiedDate from dbo.SERIALLOTNUMBERS where SERIALLOTNO like @pattern and MATERIALID=@materialID", new SqlParameter("@pattern", queryString), new SqlParameter("@materialID",material.ID)).ToList().OrderByDescending(x => x.SerialLotNo).FirstOrDefault();
                    }
                    foreach (var item in templatepattern.Where(x => x.Fixed == false).OrderByDescending(x => x.Order))
                    {
                        StringBuilder builder = new StringBuilder(resultString);
                        if (serials != null)
                        {
                            incrementalValue = serials.SerialLotNo.Substring(item.StartIndex, item.Lenght);
                            double incrementalValueInt = Convert.ToDouble(incrementalValue);
                            if (incrementalValueInt + amount > Convert.ToDouble(incrementalLastValue))
                            {
                                if (templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order) != null)
                                {
                                    builder.Remove(item.StartIndex, item.Lenght);
                                    builder.Insert(item.StartIndex, material.SerialTemplate.Lines.Where(x => x.ID.ToString() == item.Key).FirstOrDefault().Start.ToString());
                                    resultString = builder.ToString();
                                }
                                else
                                    throw new Exception(Statics.GetResourceManager().GetString("msg_frmSerialLotTransaction_CannotCreate") + "\n\r" + Statics.GetResourceManager().GetString("msg_frmSerialLotTransaction_CannotCreateTitle"));
                            }
                            else
                            {
                                incrementalValue = AlphaNumeric.NextKeyCode(incrementalValue, SequenceType.NumericOnly, incrementalValue.Length);
                                builder.Remove(item.StartIndex, item.Lenght);
                                builder.Insert(item.StartIndex, incrementalValue);
                                foreach (var notEditedValues in templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order).OrderByDescending(x => x.Order))
                                {
                                    builder.Remove(notEditedValues.StartIndex, notEditedValues.Lenght);
                                    builder.Insert(notEditedValues.StartIndex, serials.SerialLotNo.Substring(notEditedValues.StartIndex, notEditedValues.Lenght));
                                    resultString = builder.ToString();
                                }
                                resultString = builder.ToString();
                                break;
                            }
                        }
                        else
                        {
                            incrementalValue = AlphaNumeric.NextKeyCode(item.DefaultValue, SequenceType.NumericOnly, incrementalValue.Length);
                            builder.Remove(item.StartIndex, item.Lenght);
                            builder.Insert(item.StartIndex, incrementalValue);
                            foreach (var notEditedValues in templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order).OrderByDescending(x => x.Order))
                            {
                                builder.Remove(notEditedValues.StartIndex, notEditedValues.Lenght);
                                builder.Insert(notEditedValues.StartIndex, notEditedValues.DefaultValue);
                                resultString = builder.ToString();
                            }
                            resultString = builder.ToString();
                            break;
                        }
                    }
                    if (material.SerialTemplate.Lines.Where(x => x.Property == Property.MainUnitQuantity).FirstOrDefault() != null)
                    {
                        StringBuilder subBuilder;
                        SerialTemplateLine line = material.SerialTemplate.Lines.Where(x => x.Property == Property.MainUnitQuantity).FirstOrDefault();
                        ThreeDimension tp = templatepattern.Where(x => x.Key == line.ID.ToString()).FirstOrDefault();
                        int decimalCount = GetDecimalPlaces(Convert.ToDecimal(LotCount));
                        int nonDecimalCount = GetNonDecimalPlaces(Convert.ToDecimal(LotCount));
                        if (decimalCount + nonDecimalCount > line.End.ToString().Length)
                            decimalCount = decimalCount - (nonDecimalCount + decimalCount - line.End.ToString().Length);
                        string value = Math.Round(LotCount, decimalCount, MidpointRounding.ToEven).ToString().Trim('.', ',');
                        if (line.End.ToString().Length < value.Length)
                        {
                            value.Remove(line.End.ToString().Length - 1, value.Length - line.End.ToString().Length);
                            decimalCount = decimalCount - (value.Length - line.End.ToString().Length);
                        }
                        if (line.End.ToString().Length > value.Length)
                        {
                            subBuilder = new StringBuilder(value);
                            subBuilder.Insert(0, new string('0', line.End.ToString().Length - value.Length));
                            value = subBuilder.ToString();
                        }
                        string defaultString = tp.DefaultValue;
                        defaultString=defaultString.Replace("n", decimalCount.ToString());
                        subBuilder = new StringBuilder(defaultString);
                        subBuilder.Remove(line.Start.Length, defaultString.Length - line.Start.Length);
                        defaultString = subBuilder.ToString() + value;
                        subBuilder = new StringBuilder(resultString);
                        subBuilder.Remove(tp.StartIndex, tp.Lenght);
                        subBuilder.Insert(tp.StartIndex, defaultString);
                        resultString = subBuilder.ToString();
                    }
                    if (material.SerialTemplate.BarcodeType == BarcodeType.EAN13)
                        resultString = resultString + Object.HelperLayer.Functions.CalculateEAN13ControlKey(resultString);
                    SerialLotNumber serialLot = new SerialLotNumber();
                    serialLot.MaterialID = material.ID;
                    serialLot.SerialLotNo = resultString;
                    serialLot.Size = LotCount;
                    if (material.TraceType == TraceType.PartNumber)
                        serialLot.TraceType = SerialLotType.PartNumber;
                    else
                        serialLot.TraceType = SerialLotType.SerialNumber;
                    material.LotCount = oldLotCount;
                    serialLot.Save();

                    transactions.Add(new SerialLotTransaction()
                    {
                        SerialLotNumberID = serialLot.ID,
                        SerialLotNumber = (SerialLotNumber)serialLot.Clone(),
                        Amount = serialLot.Size,
                        MaterialFicheLineID = 0,
                        MovementType = serialLotTransactionMovementType,
                        ExpirationDate = DateTime.Now.AddDays(Convert.ToSingle(material.ExpirationTime))
                    });
                }
                if (remaining > 0)
                {
 
                    ///<summary>
                    /// 25.07.2019 - Uğur Can BALCI
                    /// Eğer, parti büyüklüğünden daha az bir miktar mal depoya alıyor isek, temlatepattern oluşmuyordu dolayısı ile seri numarası üretemiyorduk.
                    /// Aşağıdaki if yapısı ile, daha önce bir templatepattern oluşup oluşmadığını kontrol ettik.
                    /// Eğer template pattern oluşmuş ise bir quertString oluşmuştur. Örneğin Gy-0719_________ gibi. Ama templatePattern null ya da count==0 ise, böyle bir durumda queryString="" olacaktır ve seri boş oluşacaktır.
                    /// </summary>
                    if(templatepattern==null || templatepattern.Count==0)
                    {
                        resultString = "";
                        fixedValue = string.Empty;
                        incrementalValue = "000000000";
                        incrementalLastValue = "0";
                        queryString = "";
                        if (material.SerialTemplate == null)
                            material.SerialTemplate = virames<SerialTemplate>.Initialize(new SerialTemplate()).Where(x => x.Default == true).Take();
                        foreach (var item in material.SerialTemplate.Lines)
                        {
                            if (item.TemplateLineType == TemplateLineType.Fixed)
                            {
                                switch (item.Property)
                                {
                                    case Property.Uncertain:
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = item.Start.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = item.Start.ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += item.Start.ToString();
                                        fixedValue += item.Start.ToString();
                                        queryString += item.Start.ToString();
                                        break;
                                    case Property.MaterialID:
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = material.ID.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = material.ID.ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += material.ID.ToString();
                                        fixedValue += material.ID.ToString();
                                        queryString += material.ID.ToString();
                                        break;
                                    case Property.MaterialType:
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = material.MaterialTypeID.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = material.MaterialTypeID.ToString().Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += material.MaterialTypeID.ToString();
                                            fixedValue += material.MaterialTypeID.ToString();
                                            queryString += material.MaterialTypeID.ToString();
                                        }
                                        break;
                                    case Property.MaterialTraceType:
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = ((int)material.TraceType).ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ((int)material.TraceType).ToString().Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += ((int)material.TraceType).ToString();
                                            fixedValue += ((int)material.TraceType).ToString();
                                            queryString += ((int)material.TraceType).ToString();
                                        }
                                        break;
                                    case Property.WorkareaID:
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = Statics.GetWorkarea().ID.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = Statics.GetWorkarea().ID.ToString().Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += Statics.GetWorkarea().ID.ToString();
                                            fixedValue += Statics.GetWorkarea().ID.ToString();
                                            queryString += Statics.GetWorkarea().ID.ToString();
                                        }
                                        break;
                                    case Property.Second:
                                        if (DateTime.Now.Second < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Second.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Second.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Second.ToString();
                                            fixedValue += "0" + DateTime.Now.Second.ToString();
                                            queryString += "0" + DateTime.Now.Second.ToString();
                                        }
                                        else
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Second.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Second.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Second.ToString();
                                            fixedValue += DateTime.Now.Second.ToString();
                                            queryString += DateTime.Now.Second.ToString();
                                        }
                                        break;
                                    case Property.Minute:
                                        if (DateTime.Now.Minute < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Minute.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Minute.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Minute.ToString();
                                            fixedValue += "0" + DateTime.Now.Minute.ToString();
                                            queryString += "0" + DateTime.Now.Minute.ToString();
                                        }
                                        else
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Minute.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Minute.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Minute.ToString();
                                            fixedValue += DateTime.Now.Minute.ToString();
                                            queryString += DateTime.Now.Minute.ToString();
                                        }
                                        break;
                                    case Property.Hour:
                                        if (DateTime.Now.Hour < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Hour.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Hour.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Hour.ToString();
                                            fixedValue += "0" + DateTime.Now.Hour.ToString();
                                            queryString += "0" + DateTime.Now.Hour.ToString();
                                        }
                                        else
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Hour.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Hour.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Hour.ToString();
                                            fixedValue += DateTime.Now.Hour.ToString();
                                            queryString += DateTime.Now.Hour.ToString();
                                        }
                                        break;
                                    case Property.Day:
                                        if (DateTime.Now.Day < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Day.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Day.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Day.ToString();
                                            fixedValue += "0" + DateTime.Now.Day.ToString();
                                            queryString += "0" + DateTime.Now.Day.ToString();
                                        }
                                        else
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Day.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Day.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Day.ToString();
                                            fixedValue += DateTime.Now.Day.ToString();
                                            queryString += DateTime.Now.Day.ToString();
                                        }
                                        break;
                                    case Property.Mounth:
                                        if (DateTime.Now.Month < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Month.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Month.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Month.ToString();
                                            fixedValue += "0" + DateTime.Now.Month.ToString();
                                            queryString += "0" + DateTime.Now.Month.ToString();
                                        }
                                        else
                                        {
                                            resultString += DateTime.Now.Month.ToString();
                                            fixedValue += DateTime.Now.Month.ToString();
                                            queryString += DateTime.Now.Month.ToString();
                                        }
                                        break;
                                    case Property.Year:
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Year.ToString().Substring(2, 2),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Year.ToString().Substring(2, 2)).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Year.ToString().Substring(2, 2);
                                            fixedValue += DateTime.Now.Year.ToString().Substring(2, 2);
                                            queryString += DateTime.Now.Year.ToString().Substring(2, 2);
                                        }
                                        break;
                                    case Property.MaterialCode:
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = material.Code.Trim(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (material.Code.Trim()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += material.Code.Trim();
                                        fixedValue += material.Code.Trim();
                                        queryString += material.Code.Trim();
                                        break;
                                    case Property.MainUnitQuantity:
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = item.Start.ToString() + item.End.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (item.Start.ToString() + item.End.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += item.Start.ToString() + item.End.ToString();
                                        fixedValue += item.Start.ToString() + item.End.ToString();
                                        queryString += item.Start.ToString().Replace('n', '_') + new String('_', item.End.ToString().Length);
                                        break;
                                }
                            }
                            else
                            {
                                templatepattern.Add(new ThreeDimension()
                                {
                                    Key = item.ID.ToString(),
                                    DefaultValue = item.Start.ToString(),
                                    Fixed = false,
                                    Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                    Lenght = (item.Start.ToString()).Length,
                                    StartIndex = fixedValue.Length == 0 ? 0 : fixedValue.Length
                                });
                                resultString += item.Start.ToString();
                                incrementalValue = item.Start.ToString();
                                incrementalLastValue = item.End.ToString();
                                queryString += new String('_', item.Start.ToString().Length);
                            }
                        }
                    }
                    SerialLotNumber serials = new SerialLotNumber();
                    using (var context = new viramesMasterContext())
                    {
                        serials = context.SerialLotNumbers.SqlQuery("select ID as ID, MATERIALID as MaterialID, TRACETYPE as TraceType, SERIALLOTNO as SerialLotNo, SIZE as Size, CREATEDBY as CreatedBy, CREATEDDATE as CreatedDate, MODIFIEDBY as ModifiedBy, MODIFIEDDATE as ModifiedDate from dbo.SERIALLOTNUMBERS where SERIALLOTNO like @pattern and MATERIALID=@materialID", new SqlParameter("@pattern", queryString), new SqlParameter("@materialID", material.ID)).ToList().OrderByDescending(x => x.SerialLotNo).FirstOrDefault();
                    }
                    foreach (var item in templatepattern.Where(x => x.Fixed == false).OrderByDescending(x => x.Order))
                    {
                        StringBuilder builder = new StringBuilder(resultString);
                        if (serials != null)
                        {
                            incrementalValue = serials.SerialLotNo.Substring(item.StartIndex, item.Lenght);
                            double incrementalValueInt = Convert.ToDouble(incrementalValue);
                            if (incrementalValueInt + amount > Convert.ToDouble(incrementalLastValue))
                            {
                                if (templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order) != null)
                                {
                                    builder.Remove(item.StartIndex, item.Lenght);
                                    builder.Insert(item.StartIndex, material.SerialTemplate.Lines.Where(x => x.ID.ToString() == item.Key).FirstOrDefault().Start.ToString());
                                    resultString = builder.ToString();
                                }
                                else
                                    throw new Exception(Statics.GetResourceManager().GetString("msg_frmSerialLotTransaction_CannotCreate") + "\n\r" + Statics.GetResourceManager().GetString("msg_frmSerialLotTransaction_CannotCreateTitle"));
                            }
                            else
                            {
                                incrementalValue = AlphaNumeric.NextKeyCode(incrementalValue, SequenceType.NumericOnly, incrementalValue.Length);
                                builder.Remove(item.StartIndex, item.Lenght);
                                builder.Insert(item.StartIndex, incrementalValue);
                                foreach (var notEditedValues in templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order).OrderByDescending(x => x.Order))
                                {
                                    builder.Remove(notEditedValues.StartIndex, notEditedValues.Lenght);
                                    builder.Insert(notEditedValues.StartIndex, serials.SerialLotNo.Substring(notEditedValues.StartIndex, notEditedValues.Lenght));
                                    resultString = builder.ToString();
                                }
                                resultString = builder.ToString();
                                break;
                            }
                        }
                        else
                        {
                            incrementalValue = AlphaNumeric.NextKeyCode(item.DefaultValue, SequenceType.NumericOnly, incrementalValue.Length);
                            builder.Remove(item.StartIndex, item.Lenght);
                            builder.Insert(item.StartIndex, incrementalValue);
                            foreach (var notEditedValues in templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order).OrderByDescending(x => x.Order))
                            {
                                builder.Remove(notEditedValues.StartIndex, notEditedValues.Lenght);
                                builder.Insert(notEditedValues.StartIndex, notEditedValues.DefaultValue);
                                resultString = builder.ToString();
                            }
                            resultString = builder.ToString();
                            break;
                        }
                    }
                    if (material.SerialTemplate.Lines.Where(x => x.Property == Property.MainUnitQuantity).FirstOrDefault() != null)
                    {
                        StringBuilder subBuilder;
                        SerialTemplateLine line = material.SerialTemplate.Lines.Where(x => x.Property == Property.MainUnitQuantity).FirstOrDefault();
                        ThreeDimension tp = templatepattern.Where(x => x.Key == line.ID.ToString()).FirstOrDefault();
                        int decimalCount = GetDecimalPlaces(Convert.ToDecimal(remaining));
                        int nonDecimalCount = GetNonDecimalPlaces(Convert.ToDecimal(remaining));
                        if (decimalCount + nonDecimalCount > line.End.ToString().Length)
                            decimalCount = decimalCount - (nonDecimalCount + decimalCount - line.End.ToString().Length);
                        string value = Math.Round(remaining, decimalCount, MidpointRounding.ToEven).ToString().Trim('.', ',');
                        if (line.End.ToString().Length < value.Length)
                        {
                            value.Remove(line.End.ToString().Length - 1, value.Length - line.End.ToString().Length);
                            decimalCount = decimalCount - (value.Length - line.End.ToString().Length);
                        }
                        if (line.End.ToString().Length > value.Length)
                        {
                            subBuilder = new StringBuilder(value);
                            subBuilder.Insert(0, new string('0', line.End.ToString().Length - value.Length));
                            value = subBuilder.ToString();
                        }
                        string defaultString = tp.DefaultValue;
                        defaultString=defaultString.Replace('n', decimalCount.ToString().ToCharArray()[0]);
                        subBuilder = new StringBuilder(defaultString);
                        subBuilder.Remove(line.Start.Length, defaultString.Length - line.Start.Length);
                        defaultString = subBuilder.ToString() + value;
                        subBuilder = new StringBuilder(resultString);
                        subBuilder.Remove(tp.StartIndex, tp.Lenght);
                        subBuilder.Insert(tp.StartIndex, defaultString);
                        resultString = subBuilder.ToString();
                    }
                    if (material.SerialTemplate.BarcodeType == BarcodeType.EAN13)
                        resultString = resultString + Object.HelperLayer.Functions.CalculateEAN13ControlKey(resultString);
                    SerialLotNumber serialLot = new SerialLotNumber();
                    serialLot.MaterialID = material.ID;
                    serialLot.SerialLotNo = resultString;
                    serialLot.Size = float.Parse(remaining.ToString());
                    serialLot.TraceType = SerialLotType.PartNumber;
                    material.LotCount = oldLotCount;
                    serialLot.Save();

                    transactions.Add(new SerialLotTransaction()
                    {
                        SerialLotNumberID = serialLot.ID,
                        SerialLotNumber = (SerialLotNumber)serialLot.Clone(),
                        Amount = serialLot.Size,
                        MaterialFicheLineID = 0,
                        MovementType = serialLotTransactionMovementType,
                        ExpirationDate = DateTime.Now.AddDays(Convert.ToSingle(material.ExpirationTime))
                    });
                }
                return transactions;
            }
            catch (Exception ex)
            {
                ex = new Exception("Number Modify Error!", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Gönderilen malzeme kartına ait , belirtilen miktar ve MovementType ile SerialLotNumber ve SerialLotTransaction kayıtları oluşturur.
        /// </summary>
        /// <param name="material">Oluşturulacak seri numaralarının malzeme bilgisi, lot büyüklüğü gibi bilgilerini barındırır ve bu bilgilere göre numara kayıtları oluşturulur.</param>
        /// <param name="createamount">Oluşturulacak miktar bilgisi verilir.</param>
        /// <param name="context">Seri numaraları ve hareketlerinin bağlı bulunacağı context bilgisi</param>
        /// <param name="serialLotTransactionMovementType">Seri numaralarının toplamlarını belirlerken esas alınan MovementType bilgisi</param>
        /// <returns>Oluşturulan Seri Lot Numaralarına ait hareket bilgilerini döndürür.</returns>
        public static List<SerialLotTransaction> CreateNumbersAndTransactionsWithContext(Material material, double createamount,viramesMasterContext context, SerialLotTransactionMovementType  serialLotTransactionMovementType)
        {
            try
            {
                List<SerialLotTransaction> transactions = new List<SerialLotTransaction>();
                double amount = 0;
                double remaining = 0;
                float oldLotCount = material.LotCount;
                float LotCount = material.LotCount;
                if (material.LotCount == 0 && material.TraceType == TraceType.PartNumber) LotCount = Convert.ToSingle(createamount);
                else if (material.TraceType == TraceType.SerialNumber)
                    LotCount = 1; ;
                if (material.TraceType == TraceType.PartNumber)
                {
                    amount = Convert.ToDouble(Math.Floor(createamount / LotCount));
                    remaining = createamount % material.LotCount;
                }
                else
                    amount = Convert.ToDouble(createamount);
                string fixedValue = string.Empty;
                string incrementalValue = "000000000";
                string incrementalLastValue = "0";
                string queryString = "";
                string resultString = "";
                List<ThreeDimension> templatepattern = new List<ThreeDimension>();
                for (int i = 0; i < amount; i++)
                {
                    resultString = "";
                    fixedValue = string.Empty;
                    incrementalValue = "000000000";
                    incrementalLastValue = "0";
                    queryString = "";
                    if (material.SerialTemplate == null)
                        material.SerialTemplate = context.SerialTemplates.Where(x => x.Default == true).FirstOrDefault();
                    foreach (var item in material.SerialTemplate.Lines)
                    {
                        if (item.TemplateLineType == TemplateLineType.Fixed)
                        {
                            switch (item.Property)
                            {
                                case Property.Uncertain:
                                    templatepattern.Add(new ThreeDimension()
                                    {
                                        Key = item.ID.ToString(),
                                        DefaultValue = item.Start.ToString(),
                                        Fixed = true,
                                        Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                        Lenght = item.Start.ToString().Length,
                                        StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                    });
                                    resultString += item.Start.ToString();
                                    fixedValue += item.Start.ToString();
                                    queryString += item.Start.ToString();
                                    break;
                                case Property.MaterialID:
                                    templatepattern.Add(new ThreeDimension()
                                    {
                                        Key = item.ID.ToString(),
                                        DefaultValue = material.ID.ToString(),
                                        Fixed = true,
                                        Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                        Lenght = material.ID.ToString().Length,
                                        StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                    });
                                    resultString += material.ID.ToString();
                                    fixedValue += material.ID.ToString();
                                    queryString += material.ID.ToString();
                                    break;
                                case Property.MaterialType:
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = material.MaterialTypeID.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = material.MaterialTypeID.ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += material.MaterialTypeID.ToString();
                                        fixedValue += material.MaterialTypeID.ToString();
                                        queryString += material.MaterialTypeID.ToString();
                                    }
                                    break;
                                case Property.MaterialTraceType:
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = ((int)material.TraceType).ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ((int)material.TraceType).ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += ((int)material.TraceType).ToString();
                                        fixedValue += ((int)material.TraceType).ToString();
                                        queryString += ((int)material.TraceType).ToString();
                                    }
                                    break;
                                case Property.WorkareaID:
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = Statics.GetWorkarea().ID.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = Statics.GetWorkarea().ID.ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += Statics.GetWorkarea().ID.ToString();
                                        fixedValue += Statics.GetWorkarea().ID.ToString();
                                        queryString += Statics.GetWorkarea().ID.ToString();
                                    }
                                    break;
                                case Property.Second:
                                    if (DateTime.Now.Second < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Second.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Second.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Second.ToString();
                                        fixedValue += "0" + DateTime.Now.Second.ToString();
                                        queryString += "0" + DateTime.Now.Second.ToString();
                                    }
                                    else
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Second.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Second.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Second.ToString();
                                        fixedValue += DateTime.Now.Second.ToString();
                                        queryString += DateTime.Now.Second.ToString();
                                    }
                                    break;
                                case Property.Minute:
                                    if (DateTime.Now.Minute < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Minute.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Minute.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Minute.ToString();
                                        fixedValue += "0" + DateTime.Now.Minute.ToString();
                                        queryString += "0" + DateTime.Now.Minute.ToString();
                                    }
                                    else
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Minute.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Minute.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Minute.ToString();
                                        fixedValue += DateTime.Now.Minute.ToString();
                                        queryString += DateTime.Now.Minute.ToString();
                                    }
                                    break;
                                case Property.Hour:
                                    if (DateTime.Now.Hour < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Hour.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Hour.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Hour.ToString();
                                        fixedValue += "0" + DateTime.Now.Hour.ToString();
                                        queryString += "0" + DateTime.Now.Hour.ToString();
                                    }
                                    else
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Hour.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Hour.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Hour.ToString();
                                        fixedValue += DateTime.Now.Hour.ToString();
                                        queryString += DateTime.Now.Hour.ToString();
                                    }
                                    break;
                                case Property.Day:
                                    if (DateTime.Now.Day < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Day.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Day.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Day.ToString();
                                        fixedValue += "0" + DateTime.Now.Day.ToString();
                                        queryString += "0" + DateTime.Now.Day.ToString();
                                    }
                                    else
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Day.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Day.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Day.ToString();
                                        fixedValue += DateTime.Now.Day.ToString();
                                        queryString += DateTime.Now.Day.ToString();
                                    }
                                    break;
                                case Property.Mounth:
                                    if (DateTime.Now.Month < 10)
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = "0" + DateTime.Now.Month.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = ("0" + DateTime.Now.Month.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += "0" + DateTime.Now.Month.ToString();
                                        fixedValue += "0" + DateTime.Now.Month.ToString();
                                        queryString += "0" + DateTime.Now.Month.ToString();
                                    }
                                    else
                                    {
                                        resultString += DateTime.Now.Month.ToString();
                                        fixedValue += DateTime.Now.Month.ToString();
                                        queryString += DateTime.Now.Month.ToString();
                                    }
                                    break;
                                case Property.Year:
                                    {
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = DateTime.Now.Year.ToString().Substring(2, 2),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (DateTime.Now.Year.ToString().Substring(2, 2)).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += DateTime.Now.Year.ToString().Substring(2, 2);
                                        fixedValue += DateTime.Now.Year.ToString().Substring(2, 2);
                                        queryString += DateTime.Now.Year.ToString().Substring(2, 2);
                                    }
                                    break;
                                case Property.MaterialCode:
                                    templatepattern.Add(new ThreeDimension()
                                    {
                                        Key = item.ID.ToString(),
                                        DefaultValue = material.Code.Trim(),
                                        Fixed = true,
                                        Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                        Lenght = (material.Code.Trim()).Length,
                                        StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                    });
                                    resultString += material.Code.Trim();
                                    fixedValue += material.Code.Trim();
                                    queryString += material.Code.Trim();
                                    break;
                                case Property.MainUnitQuantity:
                                    templatepattern.Add(new ThreeDimension()
                                    {
                                        Key = item.ID.ToString(),
                                        DefaultValue = item.Start.ToString() + item.End.ToString(),
                                        Fixed = true,
                                        Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                        Lenght = (item.Start.ToString() + item.End.ToString()).Length,
                                        StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                    });
                                    resultString += item.Start.ToString() + item.End.ToString();
                                    fixedValue += item.Start.ToString() + item.End.ToString();
                                    queryString += item.Start.ToString().Replace('n', '_') + new String('_', item.End.ToString().Length);
                                    break;
                            }
                        }
                        else
                        {
                            templatepattern.Add(new ThreeDimension()
                            {
                                Key = item.ID.ToString(),
                                DefaultValue = item.Start.ToString(),
                                Fixed = false,
                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                Lenght = (item.Start.ToString()).Length,
                                StartIndex = fixedValue.Length == 0 ? 0 : fixedValue.Length
                            });
                            resultString += item.Start.ToString();
                            incrementalValue = item.Start.ToString();
                            incrementalLastValue = item.End.ToString();
                            queryString += new String('_', item.Start.ToString().Length);
                        }
                    }
                    SerialLotNumber serials = context.SerialLotNumbers.SqlQuery("select ID as ID, MATERIALID as MaterialID, TRACETYPE as TraceType, SERIALLOTNO as SerialLotNo, SIZE as Size, CREATEDBY as CreatedBy, CREATEDDATE as CreatedDate, MODIFIEDBY as ModifiedBy, MODIFIEDDATE as ModifiedDate from dbo.SERIALLOTNUMBERS where SERIALLOTNO like @pattern and MATERIALID=@materialID", new SqlParameter("@pattern", queryString), new SqlParameter("@materialID", material.ID)).ToList().OrderByDescending(x => x.SerialLotNo).FirstOrDefault();
                    foreach (var item in templatepattern.Where(x => x.Fixed == false).OrderByDescending(x => x.Order))
                    {
                        StringBuilder builder = new StringBuilder(resultString);
                        if (serials != null)
                        {
                            incrementalValue = serials.SerialLotNo.Substring(item.StartIndex, item.Lenght);
                            double incrementalValueInt = Convert.ToDouble(incrementalValue);
                            if (incrementalValueInt + amount > Convert.ToDouble(incrementalLastValue))
                            {
                                if (templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order) != null)
                                {
                                    builder.Remove(item.StartIndex, item.Lenght);
                                    builder.Insert(item.StartIndex, material.SerialTemplate.Lines.Where(x => x.ID.ToString() == item.Key).FirstOrDefault().Start.ToString());
                                    resultString = builder.ToString();
                                }
                                else
                                    throw new Exception(Statics.GetResourceManager().GetString("msg_frmSerialLotTransaction_CannotCreate") + "\n\r" + Statics.GetResourceManager().GetString("msg_frmSerialLotTransaction_CannotCreateTitle"));
                            }
                            else
                            {
                                incrementalValue = AlphaNumeric.NextKeyCode(incrementalValue, SequenceType.NumericOnly, incrementalValue.Length);
                                builder.Remove(item.StartIndex, item.Lenght);
                                builder.Insert(item.StartIndex, incrementalValue);
                                foreach (var notEditedValues in templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order).OrderByDescending(x => x.Order))
                                {
                                    builder.Remove(notEditedValues.StartIndex, notEditedValues.Lenght);
                                    builder.Insert(notEditedValues.StartIndex, serials.SerialLotNo.Substring(notEditedValues.StartIndex, notEditedValues.Lenght));
                                    resultString = builder.ToString();
                                }
                                resultString = builder.ToString();
                                break;
                            }
                        }
                        else
                        {
                            incrementalValue = AlphaNumeric.NextKeyCode(item.DefaultValue, SequenceType.NumericOnly, incrementalValue.Length);
                            builder.Remove(item.StartIndex, item.Lenght);
                            builder.Insert(item.StartIndex, incrementalValue);
                            foreach (var notEditedValues in templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order).OrderByDescending(x => x.Order))
                            {
                                builder.Remove(notEditedValues.StartIndex, notEditedValues.Lenght);
                                builder.Insert(notEditedValues.StartIndex, notEditedValues.DefaultValue);
                                resultString = builder.ToString();
                            }
                            resultString = builder.ToString();
                            break;
                        }
                    }
                    if (material.SerialTemplate.Lines.Where(x => x.Property == Property.MainUnitQuantity).FirstOrDefault() != null)
                    {
                        StringBuilder subBuilder;
                        SerialTemplateLine line = material.SerialTemplate.Lines.Where(x => x.Property == Property.MainUnitQuantity).FirstOrDefault();
                        ThreeDimension tp = templatepattern.Where(x => x.Key == line.ID.ToString()).FirstOrDefault();
                        int decimalCount = GetDecimalPlaces(Convert.ToDecimal(LotCount));
                        int nonDecimalCount = GetNonDecimalPlaces(Convert.ToDecimal(LotCount));
                        if (decimalCount + nonDecimalCount > line.End.ToString().Length)
                            decimalCount = decimalCount - (nonDecimalCount + decimalCount - line.End.ToString().Length);
                        string value = Math.Round(LotCount, decimalCount, MidpointRounding.ToEven).ToString().Trim('.', ',');
                        if (line.End.ToString().Length < value.Length)
                        {
                            value.Remove(line.End.ToString().Length - 1, value.Length - line.End.ToString().Length);
                            decimalCount = decimalCount - (value.Length - line.End.ToString().Length);
                        }
                        if (line.End.ToString().Length > value.Length)
                        {
                            subBuilder = new StringBuilder(value);
                            subBuilder.Insert(0, new string('0', line.End.ToString().Length - value.Length));
                            value = subBuilder.ToString();
                        }
                        string defaultString = tp.DefaultValue;
                        defaultString=defaultString.Replace('n', decimalCount.ToString().ToCharArray()[0]);
                        subBuilder = new StringBuilder(defaultString);
                        subBuilder.Remove(line.Start.Length, defaultString.Length - line.Start.Length);
                        defaultString = subBuilder.ToString() + value;
                        subBuilder = new StringBuilder(resultString);
                        subBuilder.Remove(tp.StartIndex, tp.Lenght);
                        subBuilder.Insert(tp.StartIndex, defaultString);
                        resultString = subBuilder.ToString();
                    }
                    if (material.SerialTemplate.BarcodeType == BarcodeType.EAN13)
                        resultString = resultString + Object.HelperLayer.Functions.CalculateEAN13ControlKey(resultString);
                    SerialLotNumber serialLot = new SerialLotNumber();
                    serialLot.MaterialID = material.ID;
                    serialLot.SerialLotNo = resultString;
                    serialLot.Size = LotCount;
                    if (material.TraceType == TraceType.PartNumber)
                        serialLot.TraceType = SerialLotType.PartNumber;
                    else
                        serialLot.TraceType = SerialLotType.SerialNumber;
                    material.LotCount = oldLotCount;
                    context.SerialLotNumbers.Add(serialLot);
                    transactions.Add(new SerialLotTransaction()
                    {
                        SerialLotNumberID = serialLot.ID,
                        SerialLotNumber = (SerialLotNumber)serialLot.Clone(),
                        Amount = serialLot.Size,
                        MaterialFicheLineID = 0,
                        MovementType = serialLotTransactionMovementType,
                        ExpirationDate = DateTime.Now.AddDays(Convert.ToSingle(material.ExpirationTime))
                    });
                }
                if (remaining > 0)
                {
                    if(templatepattern==null || templatepattern.Count==0)
                    {
                        resultString = "";
                        fixedValue = string.Empty;
                        incrementalValue = "000000000";
                        incrementalLastValue = "0";
                        queryString = "";
                        if (material.SerialTemplate == null)
                            material.SerialTemplate = context.SerialTemplates.Where(x => x.Default == true).FirstOrDefault();
                        foreach (var item in material.SerialTemplate.Lines)
                        {
                            if (item.TemplateLineType == TemplateLineType.Fixed)
                            {
                                switch (item.Property)
                                {
                                    case Property.Uncertain:
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = item.Start.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = item.Start.ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += item.Start.ToString();
                                        fixedValue += item.Start.ToString();
                                        queryString += item.Start.ToString();
                                        break;
                                    case Property.MaterialID:
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = material.ID.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = material.ID.ToString().Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += material.ID.ToString();
                                        fixedValue += material.ID.ToString();
                                        queryString += material.ID.ToString();
                                        break;
                                    case Property.MaterialType:
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = material.MaterialTypeID.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = material.MaterialTypeID.ToString().Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += material.MaterialTypeID.ToString();
                                            fixedValue += material.MaterialTypeID.ToString();
                                            queryString += material.MaterialTypeID.ToString();
                                        }
                                        break;
                                    case Property.MaterialTraceType:
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = ((int)material.TraceType).ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ((int)material.TraceType).ToString().Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += ((int)material.TraceType).ToString();
                                            fixedValue += ((int)material.TraceType).ToString();
                                            queryString += ((int)material.TraceType).ToString();
                                        }
                                        break;
                                    case Property.WorkareaID:
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = Statics.GetWorkarea().ID.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = Statics.GetWorkarea().ID.ToString().Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += Statics.GetWorkarea().ID.ToString();
                                            fixedValue += Statics.GetWorkarea().ID.ToString();
                                            queryString += Statics.GetWorkarea().ID.ToString();
                                        }
                                        break;
                                    case Property.Second:
                                        if (DateTime.Now.Second < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Second.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Second.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Second.ToString();
                                            fixedValue += "0" + DateTime.Now.Second.ToString();
                                            queryString += "0" + DateTime.Now.Second.ToString();
                                        }
                                        else
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Second.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Second.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Second.ToString();
                                            fixedValue += DateTime.Now.Second.ToString();
                                            queryString += DateTime.Now.Second.ToString();
                                        }
                                        break;
                                    case Property.Minute:
                                        if (DateTime.Now.Minute < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Minute.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Minute.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Minute.ToString();
                                            fixedValue += "0" + DateTime.Now.Minute.ToString();
                                            queryString += "0" + DateTime.Now.Minute.ToString();
                                        }
                                        else
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Minute.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Minute.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Minute.ToString();
                                            fixedValue += DateTime.Now.Minute.ToString();
                                            queryString += DateTime.Now.Minute.ToString();
                                        }
                                        break;
                                    case Property.Hour:
                                        if (DateTime.Now.Hour < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Hour.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Hour.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Hour.ToString();
                                            fixedValue += "0" + DateTime.Now.Hour.ToString();
                                            queryString += "0" + DateTime.Now.Hour.ToString();
                                        }
                                        else
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Hour.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Hour.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Hour.ToString();
                                            fixedValue += DateTime.Now.Hour.ToString();
                                            queryString += DateTime.Now.Hour.ToString();
                                        }
                                        break;
                                    case Property.Day:
                                        if (DateTime.Now.Day < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Day.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Day.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Day.ToString();
                                            fixedValue += "0" + DateTime.Now.Day.ToString();
                                            queryString += "0" + DateTime.Now.Day.ToString();
                                        }
                                        else
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Day.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Day.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Day.ToString();
                                            fixedValue += DateTime.Now.Day.ToString();
                                            queryString += DateTime.Now.Day.ToString();
                                        }
                                        break;
                                    case Property.Mounth:
                                        if (DateTime.Now.Month < 10)
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = "0" + DateTime.Now.Month.ToString(),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = ("0" + DateTime.Now.Month.ToString()).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += "0" + DateTime.Now.Month.ToString();
                                            fixedValue += "0" + DateTime.Now.Month.ToString();
                                            queryString += "0" + DateTime.Now.Month.ToString();
                                        }
                                        else
                                        {
                                            resultString += DateTime.Now.Month.ToString();
                                            fixedValue += DateTime.Now.Month.ToString();
                                            queryString += DateTime.Now.Month.ToString();
                                        }
                                        break;
                                    case Property.Year:
                                        {
                                            templatepattern.Add(new ThreeDimension()
                                            {
                                                Key = item.ID.ToString(),
                                                DefaultValue = DateTime.Now.Year.ToString().Substring(2, 2),
                                                Fixed = true,
                                                Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                                Lenght = (DateTime.Now.Year.ToString().Substring(2, 2)).Length,
                                                StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                            });
                                            resultString += DateTime.Now.Year.ToString().Substring(2, 2);
                                            fixedValue += DateTime.Now.Year.ToString().Substring(2, 2);
                                            queryString += DateTime.Now.Year.ToString().Substring(2, 2);
                                        }
                                        break;
                                    case Property.MaterialCode:
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = material.Code.Trim(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (material.Code.Trim()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += material.Code.Trim();
                                        fixedValue += material.Code.Trim();
                                        queryString += material.Code.Trim();
                                        break;
                                    case Property.MainUnitQuantity:
                                        templatepattern.Add(new ThreeDimension()
                                        {
                                            Key = item.ID.ToString(),
                                            DefaultValue = item.Start.ToString() + item.End.ToString(),
                                            Fixed = true,
                                            Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                            Lenght = (item.Start.ToString() + item.End.ToString()).Length,
                                            StartIndex = queryString.Length == 0 ? 0 : queryString.Length
                                        });
                                        resultString += item.Start.ToString() + item.End.ToString();
                                        fixedValue += item.Start.ToString() + item.End.ToString();
                                        queryString += item.Start.ToString().Replace('n', '_') + new String('_', item.End.ToString().Length);
                                        break;
                                }
                            }
                            else
                            {
                                templatepattern.Add(new ThreeDimension()
                                {
                                    Key = item.ID.ToString(),
                                    DefaultValue = item.Start.ToString(),
                                    Fixed = false,
                                    Order = templatepattern.Count == 0 ? 1 : templatepattern.Count + 1,
                                    Lenght = (item.Start.ToString()).Length,
                                    StartIndex = fixedValue.Length == 0 ? 0 : fixedValue.Length
                                });
                                resultString += item.Start.ToString();
                                incrementalValue = item.Start.ToString();
                                incrementalLastValue = item.End.ToString();
                                queryString += new String('_', item.Start.ToString().Length);
                            }
                        }
                    }
                    SerialLotNumber serials= context.SerialLotNumbers.SqlQuery("select ID as ID, MATERIALID as MaterialID, TRACETYPE as TraceType, SERIALLOTNO as SerialLotNo, SIZE as Size, CREATEDBY as CreatedBy, CREATEDDATE as CreatedDate, MODIFIEDBY as ModifiedBy, MODIFIEDDATE as ModifiedDate from dbo.SERIALLOTNUMBERS where SERIALLOTNO like @pattern and MATERIALID=@materialID", new SqlParameter("@pattern", queryString), new SqlParameter("@materialID", material.ID)).ToList().OrderByDescending(x => x.SerialLotNo).FirstOrDefault();
                    foreach (var item in templatepattern.Where(x => x.Fixed == false).OrderByDescending(x => x.Order))
                    {
                        StringBuilder builder = new StringBuilder(resultString);
                        if (serials != null)
                        {
                            incrementalValue = serials.SerialLotNo.Substring(item.StartIndex, item.Lenght);
                            double incrementalValueInt = Convert.ToDouble(incrementalValue);
                            if (incrementalValueInt + amount > Convert.ToDouble(incrementalLastValue))
                            {
                                if (templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order) != null)
                                {
                                    builder.Remove(item.StartIndex, item.Lenght);
                                    builder.Insert(item.StartIndex, material.SerialTemplate.Lines.Where(x => x.ID.ToString() == item.Key).FirstOrDefault().Start.ToString());
                                    resultString = builder.ToString();
                                }
                                else
                                    throw new Exception(Statics.GetResourceManager().GetString("msg_frmSerialLotTransaction_CannotCreate") + "\n\r" + Statics.GetResourceManager().GetString("msg_frmSerialLotTransaction_CannotCreateTitle"));
                            }
                            else
                            {
                                incrementalValue = AlphaNumeric.NextKeyCode(incrementalValue, SequenceType.NumericOnly, incrementalValue.Length);
                                builder.Remove(item.StartIndex, item.Lenght);
                                builder.Insert(item.StartIndex, incrementalValue);
                                foreach (var notEditedValues in templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order).OrderByDescending(x => x.Order))
                                {
                                    builder.Remove(notEditedValues.StartIndex, notEditedValues.Lenght);
                                    builder.Insert(notEditedValues.StartIndex, serials.SerialLotNo.Substring(notEditedValues.StartIndex, notEditedValues.Lenght));
                                    resultString = builder.ToString();
                                }
                                resultString = builder.ToString();
                                break;
                            }
                        }
                        else
                        {
                            incrementalValue = AlphaNumeric.NextKeyCode(item.DefaultValue, SequenceType.NumericOnly, incrementalValue.Length);
                            builder.Remove(item.StartIndex, item.Lenght);
                            builder.Insert(item.StartIndex, incrementalValue);
                            foreach (var notEditedValues in templatepattern.Where(x => x.Fixed == false && x.Key != item.Key && x.Order < item.Order).OrderByDescending(x => x.Order))
                            {
                                builder.Remove(notEditedValues.StartIndex, notEditedValues.Lenght);
                                builder.Insert(notEditedValues.StartIndex, notEditedValues.DefaultValue);
                                resultString = builder.ToString();
                            }
                            resultString = builder.ToString();
                            break;
                        }
                    }
                    if (material.SerialTemplate.Lines.Where(x => x.Property == Property.MainUnitQuantity).FirstOrDefault() != null)
                    {
                        StringBuilder subBuilder;
                        SerialTemplateLine line = material.SerialTemplate.Lines.Where(x => x.Property == Property.MainUnitQuantity).FirstOrDefault();
                        ThreeDimension tp = templatepattern.Where(x => x.Key == line.ID.ToString()).FirstOrDefault();
                        int decimalCount = GetDecimalPlaces(Convert.ToDecimal(remaining));
                        int nonDecimalCount = GetNonDecimalPlaces(Convert.ToDecimal(remaining));
                        if (decimalCount + nonDecimalCount > line.End.ToString().Length)
                            decimalCount = decimalCount - (nonDecimalCount + decimalCount - line.End.ToString().Length);
                        string value = Math.Round(remaining, decimalCount, MidpointRounding.ToEven).ToString().Trim('.', ',');
                        if (line.End.ToString().Length < value.Length)
                        {
                            value.Remove(line.End.ToString().Length - 1, value.Length - line.End.ToString().Length);
                            decimalCount = decimalCount - (value.Length - line.End.ToString().Length);
                        }
                        if (line.End.ToString().Length > value.Length)
                        {
                            subBuilder = new StringBuilder(value);
                            subBuilder.Insert(0, new string('0', line.End.ToString().Length - value.Length));
                            value = subBuilder.ToString();
                        }
                        string defaultString = tp.DefaultValue;
                        defaultString=defaultString.Replace('n', decimalCount.ToString().ToCharArray()[0]);
                        subBuilder = new StringBuilder(defaultString);
                        subBuilder.Remove(line.Start.Length, defaultString.Length - line.Start.Length);
                        defaultString = subBuilder.ToString() + value;
                        subBuilder = new StringBuilder(resultString);
                        subBuilder.Remove(tp.StartIndex, tp.Lenght);
                        subBuilder.Insert(tp.StartIndex, defaultString);
                        resultString = subBuilder.ToString();
                    }
                    if (material.SerialTemplate.BarcodeType == BarcodeType.EAN13)
                        resultString = resultString + Object.HelperLayer.Functions.CalculateEAN13ControlKey(resultString);
                    SerialLotNumber serialLot = new SerialLotNumber();
                    serialLot.MaterialID = material.ID;
                    serialLot.SerialLotNo = resultString;
                    serialLot.Size = float.Parse(remaining.ToString());
                    serialLot.TraceType = SerialLotType.PartNumber;
                    material.LotCount = oldLotCount;
                    context.SerialLotNumbers.Add(serialLot);
                    transactions.Add(new SerialLotTransaction()
                    {
                        SerialLotNumberID = serialLot.ID,
                        SerialLotNumber = (SerialLotNumber)serialLot.Clone(),
                        Amount = serialLot.Size,
                        MaterialFicheLineID = 0,
                        MovementType = serialLotTransactionMovementType,
                        ExpirationDate = DateTime.Now.AddDays(Convert.ToSingle(material.ExpirationTime))
                    });
                }
                return transactions;
            }
            catch (Exception ex)
            {
                ex = new Exception("Number Modify Error!", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Seri numarası oluştururken, ağırlık bilgisinde, virgülden sonra kaç basamağın var olduğunu bulan fonksiyon.
        /// </summary>
        /// <param name="n">İncelenecek sayı</param>
        /// <returns>n Sayısındaki, virgülden sonraki basamak sayısı</returns>
        private static int GetDecimalPlaces(decimal n)
        {
            try
            {
                n = Math.Abs(n); //make sure it is positive.
                n -= (int)n;     //remove the integer part of the number.
                var decimalPlaces = 0;
                while (n > 0)
                {
                    decimalPlaces++;
                    n *= 10;
                    n -= (int)n;
                }
                return decimalPlaces;
            }
            catch (Exception ex)
            {
                ex = new Exception(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 10.07.2019 - Uğur Can BALCI\n
        /// Seri numarası oluştururken, ağırlık bilgisinde, virgülden önce kaç basamağın var olduğunu bulan fonksiyon.
        /// </summary>
        /// <param name="n">İncelenecek sayı</param>
        /// <returns>n Sayısındaki, virgülden önceki basamak sayısı</returns>
        private static int GetNonDecimalPlaces(decimal n)
        {
            try
            {
                if (n < 1)
                    return 0;
                if (n/10 < 1)
                    return 1;
                else
                    return 1 + GetNonDecimalPlaces(n / 10);
            }
            catch (Exception ex)
            {
                ex = new Exception(ex.Message, ex);
                throw ex;
            }
        }
    }
}
