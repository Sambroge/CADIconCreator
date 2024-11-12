using System;
using TFlex;
using TFlex.Model;
using TFlex.Model.Model3D;
using TFlex.Command;
using System.IO;
using System.Collections.Generic;
using Res = CADIconCreator.Properties.Resources;

namespace CADIconCreator
{
    enum CommandNames
    {
        CreateIcon = 1,
    }
    public class Factory : PluginFactory
    {
        public override Plugin CreateInstance()
        {
            return new IconCreatorPlugin(this);
        }
        public override Guid ID
        {
            get
            {
                return new Guid("{025783EF-C1C4-4057-B5E2-BC9368705363}");
            }
        }
        public override string Name
        {
            get
            {
                return Res.PluginName;
            }
        }
    };
    public class IconCreatorPlugin : Plugin
    {
        private List<Construction3D> hiddenObjects;
        private RibbonGroup _group;
        private Document _document;
        private UserControl1 _control;
        public static PluginFloatingWindow FloatingWindow { get; internal set; }
        public IconCreatorPlugin(PluginFactory Factory) : base(Factory)
        {

        }
        /// <summary>
        /// Загрузка иконок
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private System.Drawing.Icon LoadIconResource(string name)
        {
            try
            {
                System.IO.Stream stream = GetType().Assembly.GetManifestResourceStream("CADIconCreator.Resources." + name + ".ico");

                return new System.Drawing.Icon(stream);
            }
            catch
            {
                return null;
            }
        }
        protected override void TrackingContextPopupMenuEventHandler(TrackingContextPopupMenuEventArgs args)
        {
        }
        /// <summary>
        /// Этот метод вызывается в тот момент, когда следует зарегистрировать команды,
        /// Создать панель, вставить пункты меню
        /// </summary>
        protected override void OnCreateTools()
        {

            RegisterCommand((int)CommandNames.CreateIcon, Res.PluginName, LoadIconResource("Icon"), LoadIconResource("Icon"));

            RibbonTab rb = new RibbonTab(Res.PluginName, this.ID, this);

            // Для добавления команд во вкладку "Приложения"

            //RibbonGroup ribbongroup = RibbonBar.ApplicationsTab.AddGroup("Создание иконок");
            //ribbongroup.AddButton((int)CommandNames.CreateIcon, "Создать иконку", this, RibbonButtonStyle.LargeIconAndCaption);

            _group = rb.AddGroup(Res.GroupName);
            _group.AddButton((int)CommandNames.CreateIcon, Res.ButtonName, this, RibbonButtonStyle.LargeIconAndCaption);

            //На случай если плагин был подключён, когда документ был уже создан и открыт (все места для AttachPlugin() пропущены),
            //просто подключаем плагин к текущему документу
            if (TFlex.Application.ActiveDocument != null) TFlex.Application.ActiveDocument.AttachPlugin(this);

            //CreateWindow();
        }
        //public void CreateWindow()
        //{
        //    if (FloatingWindow != null)
        //        return;

        //    FloatingWindow = CreateFloatingWindow(0, "Параметры создания иконки");
        //    FloatingWindow.Caption = "Создание иконки";
        //    FloatingWindow.Icon = LoadIconResource("Icon");
        //    FloatingWindow.Visible = false;
        //}

        //protected override System.Windows.Forms.Control CreateFloatingWindowControl(uint id)
        //{
        //    //if (id != 0)
        //    //    return null;
        //    _control = new UserControl1();
        //    return _control;
        //    //До загрузки докса, нужно создать панель заглушку иначе будет падение при инициализации окна CAD
        //    return new System.Windows.Forms.Panel();
        //}

        /// <summary>
        /// Обработка команд от панели и главного меню
        /// </summary>
        /// <param name="document"></param>
        /// <param name="id"></param>
        protected override void OnCommand(Document document, int id)
        {
            base.OnCommand(document, id);

            switch ((CommandNames)id)
            {
                case CommandNames.CreateIcon:
                    hiddenObjects = new List<Construction3D>();
                    document.BeginChanges(Res.BeginChangesTitle);
                    foreach (var obj in document.GetObjects())
                        if (obj is Construction3D con)
                        {
                            if (con.VisibleInScene == true)
                            {
                                hiddenObjects.Add(con);
                                con.VisibleInScene = false;
                            }
                        }
                    if (CreateIcon(document))
                    {
                        foreach (Construction3D con in hiddenObjects) con.VisibleInScene = true;
                        document.EndChanges();
                    }
                    else
                    {
                        document.CancelChanges();
                    }
                    //FloatingWindow.Visible = !FloatingWindow.Visible;
                    hiddenObjects.Clear();
                    hiddenObjects = null;
                    break;
            }

        }
        private bool CreateIcon(Document document)
        {
            string path;
            ExportToBitmap3D param = new ExportToBitmap3D(document.ActiveView)
            {
                Height = 256,
                Width = 256,
                PixelSize = true,
                Annotations = false,
                Constructions = false,
                Dpi = 72,
            };
            string tempPath = Path.GetTempPath();
            try
            {
                path = tempPath + document.FileName.Replace(document.FilePath, "").Replace(".grb", ".bmp");
            }
            catch
            {
                document.Diagnostics.Add(new DiagnosticsMessage(DiagnosticsMessageType.Warning, Res.NotSavedDocumentError + this.Name));
                return false;
            }
            param.Export(path);
            document.ImportIcon(path);
            DeleteTempFile(path, document);
            return true;
        }
        private void DeleteTempFile(string path, Document document)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException ex)
            {
                document.Diagnostics.Add(new DiagnosticsMessage(DiagnosticsMessageType.Warning, Res.FileDeletingError + path + ". Плагин " + this.Name));
            }
        }
        /// <summary>
        /// Здесь можно блокировать команды и устанавливать галочки
        /// </summary>
        /// <param name="cmdUI"></param>
        protected override void OnUpdateCommand(CommandUI cmdUI)
        {
            if (cmdUI == null)
                return;

            if (cmdUI.Document == null)
            {
                cmdUI.Enable(false);
                return;
            }
        }
    }
}
