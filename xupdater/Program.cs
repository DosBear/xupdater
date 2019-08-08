using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace xupdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string oper, nspace, path1, path2, path3, filename1, filename2;
            XmlDocument xdoc;

            Console.WriteLine("Версия {0}", Const.APP.VERSION);
            Console.WriteLine();
            warning("Выбирите операцию:");
            Console.WriteLine("\t[1] Сбор информации о xsd-схемах ИРС ВЭД");
            Console.WriteLine("\t[2] Поиск подходящих схем проверки");
            Console.WriteLine("\t[3] Обновление пространст имен");
            Console.WriteLine("Укажите номер проводимой операции");
            oper = Console.ReadLine();
            if (oper.Length != 1 || "123".IndexOf(oper) < 0)
            {
                Console.WriteLine("Ошибка в указании выбранной операции");
                return;
            }
            switch (oper)
            {
                case "1":
                    Console.WriteLine("Укажите каталог схем ИРС ВЭД");
                    path1 = @"\\Mac\Home\Desktop\SCHEMAS"; //Console.ReadLine();
                    if (!Directory.Exists(path1))
                    {
                        Console.WriteLine("Директория {0} не найдена", path1);
                        return;
                    }
                    DirectoryInfo dirinfo = new DirectoryInfo(path1);
                    xdoc = new XmlDocument();
                    path2 = Path.Combine(Const.APP.APPDIR, Const.APP.XSD_SETTINGS_FILE);
                    if (File.Exists(path2)) File.Delete(path2);
                    using (XmlTextWriter wr = new XmlTextWriter(path2, Encoding.UTF8))
                    {
                        wr.Formatting = System.Xml.Formatting.Indented;
                        wr.WriteStartDocument();
                        wr.WriteStartElement("XSD_FILES");
                        int i = 0;
                        foreach (FileInfo file in dirinfo.GetFiles("*.xsd"))
                        {
                            try
                            {
                                i++;
                                xdoc.Load(file.FullName);
                                nspace = xdoc.ChildNodes[1].Attributes["targetNamespace"].InnerText;
                                nspace = nspace.Split(':')[nspace.Split(':').Length - 2];
                                wr.WriteStartElement("XSD");
                                wr.WriteElementString("FILE", file.Name.ToUpper());
                                wr.WriteElementString("NSPACE", nspace.ToUpper() + ".XSD");
                                wr.WriteEndElement(); //XSD
                                Console.Write("{0}) {1} {2} \t", i, file.Name, nspace);
                                ok();
                            }
                            catch (Exception err)
                            {
                                Cancel();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Во время обработки файла {0} произошла ошибка {1}.", file.Name, err.Message);
                            }
                        }
                        wr.WriteEndElement(); //XSD_FILES
                    }

                    Console.ReadLine();

                    break;
                case "2":
                    Console.WriteLine("Укажите каталог для поиска схем проверок");
                    path1 = @"V:\Формат\2019-07-1_3\Schemas_out_5_14_3_18062019";  //Console.ReadLine();
                    if (!Directory.Exists(path1))
                    {
                        Console.WriteLine("Директория {0} не найдена", path1);
                        return;
                    }
                    Console.WriteLine("Укажите каталог выгрузки новых XSD схем ИРС ВЭД");
                    path2 = @"v:\22";//Console.ReadLine();
                    if (!Directory.Exists(path2))
                    {
                        Console.WriteLine("Директория {0} не найдена", path2);
                        return;
                    }
                    path3 =Path.Combine(Const.APP.APPDIR, Const.APP.XSD_SETTINGS_FILE);
                    if (!File.Exists(path3))
                    {
                        Console.WriteLine("Файл настроек {0} не найден", path3);
                        return;
                    }
                    xdoc = new XmlDocument();
                    xdoc.Load(path3);
                    Dictionary<string, string> XSDDICT = new Dictionary<string, string>();
                    warning("Поиск и перенос файлов:");
                    foreach (XmlNode node in xdoc["XSD_FILES"].ChildNodes)
                    {
                        if (File.Exists(Path.Combine(path1, node["NSPACE"].InnerText)))
                        {
                            File.Copy(Path.Combine(path1, node["NSPACE"].InnerText ),
                                        Path.Combine(path2, node["FILE"].InnerText), true);

                            Console.Write("{0} \t", node["FILE"].InnerText.ToUpper());
                            ok();
                            try
                            {

                                XSDDICT.Add(node["NSPACE"].InnerText, node["FILE"].InnerText);
                            }
                            catch (Exception err)
                            {
                                warning("Дублирование для файла " + node["FILE"].InnerText);
                            }
                        } else
                        {
                            Console.WriteLine("Файл {0} не найден", Path.Combine(path1, node["NSPACE"].InnerText));
                        }
                    }
                    warning("Обновление schemaLocation:");
                    foreach (var vals in XSDDICT)
                    {
                        Console.WriteLine("Файл {0} \t", vals.Value);
                        xdoc.Load(Path.Combine(path2, vals.Value));
                        foreach (XmlNode node in xdoc.SelectNodes("//*[local-name()='import']"))
                        {
                            Console.Write("\t schemaLocation {0} \t", node.Attributes["schemaLocation"].InnerText);
                            if (XSDDICT.ContainsKey(node.Attributes["schemaLocation"].InnerText.ToUpper()))
                            {
                                node.Attributes["schemaLocation"].InnerText = XSDDICT[node.Attributes["schemaLocation"].InnerText.ToUpper()];
                                ok();
                            }
                            else
                            {
                                Cancel();
                            }
                        }
                    }
                    Console.ReadLine();
                    break;
                default:
                    break;
            }

        }

        private static void warning(string str)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        private static void ok(string tmp = "OK")
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\t" + tmp + "\r\n");
            Console.ResetColor();
        }

        private static void Cancel(string tmp = "Отмена")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\t" + tmp + "\r\n");
            Console.ResetColor();
        }
    }
}
