using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADIconCreator
{
    class APILoader
    {
        static public void Initialize()
        {
            if (_folders != null)
                return;

            _folders = new List<string>();

            //var assemblyConfigurationAttribute = typeof(Program).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            //var buildConfigurationName = assemblyConfigurationAttribute?.Configuration;

            string path = "";
#if Debug17
                path = GetPath(@"T-FLEX CAD 3D 17\Rus");
#endif

#if Debug18
                path = GetPath(@"T-FLEX CAD 3D 18\Rus");
#endif

            //string path = GetPath(@"T-FLEX CAD 3D 18\Rus");
            if (string.IsNullOrEmpty(path))
                throw new System.IO.FileNotFoundException("T-FLEX CAD not installed");

            _folders.Add(path);

            /*path = GetPath(@"T-FLEX DOCs 17\Rus");

			if (string.IsNullOrEmpty(path))
				throw new System.IO.FileNotFoundException("T-FLEX DOCs not installed");

			_folders.Add(path);*/

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
        }

        static public bool InitializeTFlexCADAPI()
        {
            if (_folders == null)
                throw new InvalidOperationException("Call Initialize first");

            //Перед работой с API T-FLEX CAD его необходимо инициализировать
            //В зависимости от параметров инициализации, будут или не будут
            //доступны функции изменения документов и сохранение документов в файл.
            //За это отвечает параметр setup.ReadOnly.
            //Если setup.ReadOnly = false, то для работы программы требуется
            //лицензия на сам T-FLEX CAD
            TFlex.ApplicationSessionSetup setup = new TFlex.ApplicationSessionSetup();
            setup.ReadOnly = false;
            //setup.EnableDOCs = true;
            //setup.DOCsProductName = "T-FLEX DOCs 17";
            return TFlex.Application.InitSession(setup);
        }

        static public void Terminate()
        {
            if (_folders == null)
                return;

            TFlex.Application.ExitSession();

            _folders = null;
        }

        static private List<string> _folders;

        static private string GetPath(string product)
        {
            if (string.IsNullOrEmpty(product))
                return "";

            string regPath = string.Format(@"SOFTWARE\Top Systems\{0}\", product);

            RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
            if (key == null)
                return "";

            string path = (string)key.GetValue("ProgramFolder", string.Empty);
            if (string.IsNullOrEmpty(path))
                path = (string)key.GetValue("SetupHelpPath", string.Empty);

            key.Close();

            if (path.Length > 0 && path[path.Length - 1] != '\\')
                path += @"\";

            return path;
        }

        static private System.Reflection.Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (_folders == null || _folders.Count == 0)
                return null;

            try
            {
                string name = args.Name;

                int index = name.IndexOf(",");
                if (index > 0)
                    name = name.Substring(0, index);

                foreach (string path in _folders)
                {
                    if (!System.IO.Directory.Exists(path))
                        continue;

                    string fileName = string.Format("{0}{1}.dll", path, name);

                    if (!System.IO.File.Exists(fileName))
                        continue;

                    System.IO.Directory.SetCurrentDirectory(path);

                    return System.Reflection.Assembly.LoadFile(fileName);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Ошибка загрузки сборки {0}.\n\nОписание:\n{1}", args.Name, ex.Message),
                    "Ошибка", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }

            return null;
        }
    }
}
