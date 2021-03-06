﻿using System;
using System.ComponentModel;

namespace Bio.Framework.Client.SL {
  /// <summary>
  /// Класс, описывающий общие настройки приложения.
  /// </summary>
  [Description("Приложение")]
  public class ConfigRoot : ConfigRec, IConfigRoot {
    /// <summary>
    /// Имя пользователя, входившего последним в программу.
    /// </summary>
    public String LastLoggedInUserName { get; set; }
    public String LastLoggedInUserPwd { get; set; }
      
    /// <summary>
    /// Время ожидания ответа от сервера в секундах.
    /// </summary>
    [Category("Соединение")]
    [Description("Время ожидания")]
    [DefaultValue(60)]
    public Int32 RequestTimeout { get; set; }

    /// <summary>
    /// Автоматически подключаться при запуске программы.
    /// </summary>
    [Category("Соединение")]
    [Description("Автоподключение")]
    [DefaultValue(true)]
    public Boolean AutoConnect { get; set; }

    [Category("Соединение")]
    [Description("Поддерживать соединение")]
    [DefaultValue(false)]
    public Boolean BeOnline { get; set; }

    [Category("Обновление")]
    [Description("Отложить загрузку данных в таблицах")]
    [DefaultValue(false)]
    public Boolean SuspendLoadDataInGrids { get; set; }

    /// <summary>
    /// Сохранить пароль
    /// </summary>
    [Category("Вход")]
    [Description("Запоминать пароль")]
    [DefaultValue(false)]
    public Boolean SavePassword { get; set; }

    [Category("Обновление")]
    [Description("Кешировать значения выпадающих списков")]
    [DefaultValue(false)]
    public Boolean CacheCbxItems { get; set; }

    /// <summary>
    /// Проверка значения
    /// </summary>
    /// <returns></returns>
    public override Boolean ValidateCfg() {
      return true;
    }
  }


}
