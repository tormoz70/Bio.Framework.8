﻿namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Framework.Packets;
  using System.ComponentModel;
  using Bio.Helpers.Common.Types;
  using System.IO;
  using Bio.Helpers.Common;

  /// <summary>
  /// Предоставляет возможность запускать на сервере долгоиграющие отчеты.
  /// </summary>
  public class CExpClient:CRmtClientBase {


    /// <summary>
    /// Конструктор класса.
    /// </summary>
    /// <param name="ajaxMng">ссылка на AjaxMng</param>
    /// <param name="bioCode">Код</param>
    /// <param name="title">Заголовок</param>
    public CExpClient(IAjaxMng ajaxMng, String bioCode, String title):base(ajaxMng, bioCode, title) {
      this._requestType = RequestType.DS2XL;
    }


  }
}
