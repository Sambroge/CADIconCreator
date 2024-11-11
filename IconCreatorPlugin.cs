using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFlex;
using TFlex.Model;
using TFlex.Model.Model3D;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using TFlex.Command;

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
                return "Создание иконок";
            }
        }
    };

    public class IconCreatorPlugin : Plugin
    {
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

            RegisterCommand((int)CommandNames.CreateIcon, "Создание иконок", LoadIconResource("Icon"), LoadIconResource("Icon"));

            RibbonTab rb = new RibbonTab("Создание иконок", this.ID, this);

            // Для добавления команд во вкладку "Приложения"
            RibbonGroup ribbongroup = RibbonBar.ApplicationsTab.AddGroup("Создание иконок");
            ribbongroup.AddButton((int)CommandNames.CreateIcon, "Создать иконку", this, RibbonButtonStyle.SmallIcon);

            _group = rb.AddGroup("Иконки");
            _group.AddButton((int)CommandNames.CreateIcon, "Создать иконку", this, RibbonButtonStyle.LargeIconAndCaption);

            //На случай если плагин был подключён, когда документ был уже создан и открыт (все места для AttachPlugin() пропущены),
            //просто подключаем плагин к текущему документу
            if (TFlex.Application.ActiveDocument != null) TFlex.Application.ActiveDocument.AttachPlugin(this);

            CreateWindow();
        }

        public void CreateWindow()
        {
            if (FloatingWindow != null)
                return;

            FloatingWindow = CreateFloatingWindow(0, "Параметры создания иконки");
            FloatingWindow.Caption = "Создание иконки";
            FloatingWindow.Icon = LoadIconResource("Icon");
            FloatingWindow.Visible = false;
        }

        protected override System.Windows.Forms.Control CreateFloatingWindowControl(uint id)
        {
            //if (id != 0)
            //    return null;
            _control = new UserControl1();
            return _control;
            //До загрузки докса, нужно создать панель заглушку иначе будет падение при инициализации окна CAD
            return new System.Windows.Forms.Panel();
        }

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
                    FloatingWindow.Visible = !FloatingWindow.Visible;
                   break;
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
