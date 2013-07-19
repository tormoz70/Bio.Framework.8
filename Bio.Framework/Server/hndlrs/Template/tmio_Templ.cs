namespace Bio.Framework.Server {

  using System;
  using System.Data;
  using System.Data.Common;

  using System.Collections.Specialized;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Bio.Helpers.Common.Types;

  /// <summary>
  /// Данный обработчик вх. сообщений является шаблоном для создания нового сообщения
  /// Обработчик запросов на отображение таблицы
  /// </summary>
  public class tmio_Templ : ABioHandlerBio {

    public tmio_Templ(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    protected override void doExecute() {
      base.doExecute();

    }
  }
}
