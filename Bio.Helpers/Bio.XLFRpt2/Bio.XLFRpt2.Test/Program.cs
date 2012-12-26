using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.XLFRpt2;
using Bio.Helpers.Common.Types;
using Bio.Helpers.XLFRpt2.Engine;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;
//using Bio.Helpers.XLFRpt2.Ipc;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using Bio.Helpers.Common;
using System.Data;
//using Bio.Helpers.XLFRpt2.Ipc;
using System.Xml;
using System.IO;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Converters;
//using Bio.Framework.Packets;

namespace Bio.XLFRpt2.Test {
  //public class Cilgen {
  //  private Int64? _val;
  //  public Int64? val {
  //    get {
  //      return this._val;
  //    }
  //    set {
  //      this.setProp(this._val, value, "sdf");
  //    }
  //  }
  //  private void setProp(Object oldVal, Object newVal, String propName) {
  //    //return !Object.Equals(oldVal, newVal);
  //  }
  //}

  //public class Dtest {
  //  public DateTime vald1 { get; set; }
  //  //public DateTimeOffset vald2 { get; set; }
  //  //public Object[] val { get; set; }
  //}

  class Program {
    static void Main(string[] args) {
      
      //testReport();
      testReport1();
      //testCQueueRemote();
      //testCQueueRemoteGetResult();
      //testCQueueRemoteGetRptTreeNode();
      //String s = Utl.GetEnumFieldDesc<XLReportState>(XLReportState.rsError);
      //s = s + "!";
      //String vStr = "самоход";
      //Byte[] buf = Encoding.UTF8.GetBytes(vStr);
      //String vStrUTF = Encoding.UTF8.GetString(buf);
      
      //Int64? vii = 45;
      //Cilgen vvv = new Cilgen();
      //vvv.val = vii;
      //String vvv = "p_param";
      //Utl.regexReplace(ref vvv, "^\\bp_", String.Empty, true);
      //Console.WriteLine(vvv);

      //var ddd = new Dtest {
      //  //vald1 = DateTime.SpecifyKind(DateTimeParser.Instance.ParsDateTime("20110401 12:00:00"), DateTimeKind.Utc),
      //  //vald2 = DateTime.SpecifyKind(DateTimeParser.Instance.ParsDateTime("20110401 12:00:00"), DateTimeKind.Utc),

      //  vald1 = DateTime.Now,
      //  //vald2 = DateTime.Now,
      //  //val = new Object[] { DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) }
      //};

      ////String json1 = jsonUtl.Encode(ddd, null);
      //String json2 = jsonUtl.Encode(ddd, new JsonConverter[] { new LocDateTimeConverter() });  //JsonConvert.SerializeObject(ddd, new LocDateTimeConverter());
      //Console.WriteLine(json2);

      ////String sss = "{\"$type\":\"Bio.Framework.Packets.CJsonStoreResponse, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"cmd\":0,\"packet\":{\"$type\":\"Bio.Framework.Packets.CJsonStoreData, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"isFirstLoad\":true,\"isMetadataObsolete\":true,\"start\":0,\"limit\":30,\"endReached\":false,\"totalCount\":31,\"metaData\":{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadata, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"readOnly\":true,\"fields\":{\"$type\":\"System.Collections.Generic.List`1[[Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"pk_value\",\"align\":0,\"hidden\":true,\"readOnly\":true,\"pk\":0,\"type\":1,\"width\":0,\"group\":0},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"film_id\",\"format\":\"0\",\"align\":4,\"header\":\"ID фильма\",\"hidden\":false,\"readOnly\":true,\"pk\":1,\"type\":4,\"width\":0,\"group\":-1,\"group_aggr\":\"\"},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"prnt_film_id\",\"format\":\"0\",\"align\":4,\"header\":\"ID фильма по гос реестру\",\"hidden\":false,\"readOnly\":true,\"pk\":0,\"type\":4,\"width\":0,\"group\":-1,\"group_aggr\":\"\"},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"film_name\",\"align\":1,\"header\":\"Название фильма от прокатчика\",\"hidden\":false,\"readOnly\":true,\"pk\":0,\"type\":1,\"width\":0,\"group\":-1,\"group_aggr\":\"\"},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"film_name_orig\",\"align\":1,\"header\":\"Название фильма по гос реестру\",\"hidden\":false,\"readOnly\":true,\"pk\":0,\"type\":1,\"width\":0,\"group\":-1,\"group_aggr\":\"\"},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"rec_verified\",\"align\":1,\"header\":\"Выверен\",\"hidden\":false,\"readOnly\":true,\"pk\":0,\"type\":8,\"width\":0,\"group\":-1,\"group_aggr\":\"\"},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"hide_sess\",\"align\":1,\"header\":\"Скрыть\",\"hidden\":false,\"readOnly\":false,\"pk\":0,\"type\":8,\"width\":0,\"group\":-1,\"group_aggr\":\"\"},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"cre_date\",\"format\":\"yyyy.MM.dd HH:mm:ss\",\"align\":1,\"header\":\"Дата/Время поступления\",\"hidden\":false,\"readOnly\":true,\"pk\":0,\"type\":16,\"width\":0,\"group\":-1,\"group_aggr\":\"\"},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"vrfd_date\",\"format\":\"yyyy.MM.dd HH:mm:ss\",\"align\":1,\"header\":\"Дата/Время выверки\",\"hidden\":false,\"readOnly\":true,\"pk\":0,\"type\":16,\"width\":0,\"group\":-1,\"group_aggr\":\"\"},{\"$type\":\"Bio.Framework.Packets.CJsonStoreMetadataFieldDef, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"name\":\"vrfd_usr\",\"align\":1,\"header\":\"Кто сделал выверку\",\"hidden\":false,\"readOnly\":true,\"pk\":0,\"type\":1,\"width\":0,\"group\":-1,\"group_aggr\":\"\"}]},\"PKDefined\":true},\"rows\":{\"$type\":\"Bio.Framework.Packets.CJsonStoreRows, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"$values\":[{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"8cd819854ed74545b34e2f904148b433\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(8)\",8,140497245,\"Механик\",\"Механик_\",true,false,new Date(1297094297000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"b0038a0a683d4abd87669ea629ce8671\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(9)\",9,145213253,\"4 3 2 1\",\"4.3.2.1.\",true,false,new Date(1297094298000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"f0d81b9734db4d47bb433a6c961f476e\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(10)\",10,140392919,\"Ты и Я\",\"Ты и я .\",true,false,new Date(1297094299000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"2a9b6f01fe6a4c1b99d9aaa6cabd58b1\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(11)\",11,140522819,\"Любовь и другие лекарства\",\"Любовь и другие лекарства\",true,false,new Date(1297094302000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"f73270f39422455784dac8e07b626920\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(12)\",12,140544873,\"На крючке\",\"На крючке (\\\"Номер 13\\\")\",true,false,new Date(1297094308000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"cc7fc2f6a9354c1e8e09513074257e12\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(13)\",13,140507737,\"Самый Лучший Фильм 3D\",\"Самый лучший фильм 3-Дэ\",true,false,new Date(1297094319000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"9498cb49fefe4b02a4cf3c72e5c3469d\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(14)\",14,140643825,\"Черный лебедь\",\"Черный лебедь.\",true,false,new Date(1297094321000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"9296a1f8d41b408ca356ac4cc9d3c705\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(15)\",15,140443636,\"Турист\",\"Бурлеск\",true,false,new Date(1297094322000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"6695d82622ca4def9713c9567292c3d9\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(16)\",16,139660437,\"Время ведьм\",\"Щелкунчик и крысиный король 3D\",true,false,new Date(1297094323000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"2db8250cc9944f8887a28accea7fc64b\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(5)\",5,140443636,\"Бурлеск\",\"Бурлеск\",true,false,new Date(1297094296000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"869ccb1128c647cba81b6ca9f5e01efd\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(6)\",6,140552998,\"Поцелуй сквозь стену\",\"Поцелуй сквозь стену\",true,false,new Date(1297094296000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"19426af680354e2aaa36a62e01137245\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(7)\",7,140568891,\"Зеленый Шершень 3D\",\"Зеленый шершень\",true,false,new Date(1297094297000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"e4a2b1495f1747e8800df293ac2348fb\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(21)\",21,140507737,\"Самый Лучший Фильм\",\"Самый лучший фильм 3-Дэ\",true,false,new Date(1297094471000),new Date(1298973216000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"1bb8d23b7374447689c00587a9e36a6e\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(36)\",36,110128318,\"Матрица\",\"Матрица\",true,false,new Date(1297094655000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"37d6feeeb68b40a995d59505ffcc5ae6\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(37)\",37,123841849,\"Принц Персии\",\"Принц Персии: Пески времени\",true,false,new Date(1297094656000),new Date(1299050926000),\"coordinator\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"ad9d1a49c7c1484a9d2ab2bb069e9474\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(38)\",38,0,\"ГАРРИ ПОТТЕР 7\",null,false,false,new Date(1297094656000),new Date(1298973214000),\"coordinator\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"c6b11c65104c4ff6951f22dbf0cd7637\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(39)\",39,140273432,\"Доброе утро\",\"Доброе Утро\",true,false,new Date(1297094658000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"b73e581653ec4959af14851eee1fe138\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(40)\",40,110100967,\"Зимнее утро\",\"Зимнее утро\",true,false,new Date(1297094658000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"4af7e85e33bb45b594f30869295ff128\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(48)\",48,139653156,\"Убойные каникулы (фильм ужасов)\",\"Убойные каникулы (фильм ужасов)\",true,false,new Date(1297101850000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"558161c0216a409da9b36fb88e39efe1\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(59)\",59,140129443,\"Отчаянная домохозяйка\",\"Отчаянная домохозяйка\",true,false,new Date(1297108531000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"c428b2fb3b7842e3b00068291e333298\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(62)\",62,140507737,\"Самый лучший фильм 3-ДЭ\",\"Самый лучший фильм 3-Дэ\",true,false,new Date(1297109512000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"5e7fa2929f3d4c789f73808cdabca767\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(92)\",92,140592244,\"Телохранитель\",\"Телохранитель _\",true,false,new Date(1297114201000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"0424354731d84fd5a1d34df3a5891faf\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(120)\",120,140507737,\"Самый лучший фильм ЗДЭ\",\"Самый лучший фильм 3-Дэ\",true,false,new Date(1297180949000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"e0fc77584e08474b9b0a07fbb760c5c4\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(123)\",123,110119245,\"Куда приводят мечты\",\"Куда приводят мечты\",true,false,new Date(1297236409000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"1ffc7a5009df4650b9059210910e166f\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(1571)\",1571,139615018,\"Гарри Поттер и Дары Смерти.Часть 1\",\"Гарри Поттер и Дары Смерти - Часть 1/По роману Дж.К.Ролинг/\",true,false,new Date(1298826128000),new Date(1298973264000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"b3c4cbe8cab14820945644edacf904c9\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(125)\",125,110060674,\"Без лица\",\"Без лица\",true,false,new Date(1297236694000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"116e11a985d944149868d727601009a3\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(127)\",127,124090075,\"ДВОЙНОЙ КОПЕЦ\",\"Двойной копец\",true,false,new Date(1297239004000),new Date(1298973217000),\"<автомат>\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"18d34f0bdeec4f49b28a8dc829bb377d\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(128)\",128,124042276,\"КОМАНДА А\",\"Команда-А\",true,false,new Date(1297239004000),new Date(1299050955000),\"coordinator\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"6346f2d39ba04625ad988ec20de64ac8\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(129)\",129,124034918,\"ИСТОРИЯ ИГРУШЕК: БОЛЬШОЙ ПОБЕГ 3D\",\"История игрушек: Большой побег\",true,false,new Date(1297239005000),new Date(1299050991000),\"coordinator\"]}},{\"$type\":\"Bio.Framework.Packets.CJsonStoreRow, Bio.Framework.Packets, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"internalROWUID\":\"99be3d462a3d41a887e427ca78f69bda\",\"changeType\":0,\"Values\":{\"$type\":\"System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"$values\":[\"(130)\",130,123841849,\"Принц Персии: Пески времени\",\"Принц Персии: Пески времени\",true,false,new Date(1297239006000),new Date(1298973217000),\"<автомат>\"]}}]}},\"bioParams\":{\"$type\":\"Bio.Helpers.Common.Types.CParams, Bio.Helpers.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"$values\":[{\"$type\":\"Bio.Helpers.Common.Types.CParam, Bio.Helpers.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Name\":\"verified\",\"Value\":\"0\",\"ParamType\":\"System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"ParamSize\":0,\"ParamDir\":0},{\"$type\":\"Bio.Helpers.Common.Types.CParam, Bio.Helpers.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Name\":\"filter\",\"ParamType\":\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"ParamSize\":0,\"ParamDir\":0},{\"$type\":\"Bio.Helpers.Common.Types.CParam, Bio.Helpers.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Name\":\"rnumto$\",\"Value\":31,\"ParamType\":\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"ParamSize\":0,\"ParamDir\":0},{\"$type\":\"Bio.Helpers.Common.Types.CParam, Bio.Helpers.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Name\":\"rnumfrom$\",\"Value\":0,\"ParamType\":\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"ParamSize\":0,\"ParamDir\":0}]},\"success\":true}";
      ////var ddd1 = JsonConvert.DeserializeObject(json2, typeof(Dtest), new LocDateTimeConverter());
      //var ddd1 = jsonUtl.Decode(json2, null, new JsonConverter[] { new LocDateTimeConverter() });
      //Console.WriteLine(ddd1);

    }
    /*
    static void test_detectRptAttrsByCode() {
      String descFilePath = null;
      String shrtCode = null;
      String throwCode = null;
      CXLReportConfig.detectRptAttrsByCode(
        @"D:\data\prjs\bioSys-8\Bio.Helpers\Bio.XLFRpt2\Bio.XLFRpt2.Srvc\bin\test\root\", "connLog.report",
        ref descFilePath, ref shrtCode, ref throwCode);
    }

    static void testCQueueRemote() {
      //// Create the channel.
      //IpcChannel channel = new IpcChannel();
      //// Register the channel.
      //ChannelServices.RegisterChannel(channel);
      //// Register as client for remote object.
      //WellKnownClientTypeEntry remoteType = new WellKnownClientTypeEntry(
      //        typeof(RemoteObject),
      //        "ipc://localhost:9090/RemoteObject.rem");
      //RemotingConfiguration.RegisterWellKnownClientType(remoteType);

      //NetworkInterface[] ints = NetworkInterface.GetAllNetworkInterfaces();
      //IPInterfaceProperties ippr = ints[2].GetIPProperties();

      CQueueIpcClient cli = new CQueueIpcClient();
      CParams v_prms = new CParams();
      v_prms.Add("rpt_bgn", "20100719");
      v_prms.Add("rpt_end", "20100722");
      cli.Add("connLog.report", "111", "FTW", "localhost", v_prms, ThreadPriority.Normal);
      String r = cli.GetQueue("FTW", "localhost");
      Console.WriteLine(r);
      Console.WriteLine("Press enter to stop this process");
      Console.ReadLine();
      
    }

    static void testCQueueRemoteGetResult() {
      CQueueIpcClient cli = new CQueueIpcClient();
      String vFileName = null;
      byte[] vBuffer = null;
      cli.GetReportResult("9222216DFAD6DE69E040007F01002089", null, null, ref vFileName, ref vBuffer);
      Utl.WriteBuffer2BinFile(vFileName, vBuffer);
    }

    static void testCQueueRemoteGetRptTreeNode() {
      //CQueueIpcClient cli = new CQueueIpcClient();
      //String r = cli.GetRptTreeNode(null, null, "connLog");

      XmlDocument dc = CXLRptTreeNav.buildRptTreeNav(@"D:\data\prjs\bioSys-8\Bio.Helpers\Bio.XLFRpt2\Bio.XLFRpt2.Srvc\bin\test\root\",
        "connLog.Cat.CatS.back_door", "admin");
      String r = dc.InnerXml;

      Console.WriteLine(r);
    }
    */
    static void testReport() {
      CXLReportConfig rptCfg = CXLReportConfig.LoadFromFile(
        null,
        "report",
        null,
        "logs",
        null,
        null,
        null,
        null,
        new CParams(new CParam { Name = "rpt_bgn", Value = "20100714" }, new CParam { Name = "rpt_end", Value = "20100716" }),
        false
      );
      CXLReport rptBuilder = new CXLReport(rptCfg);
      //rptBuilder.DataSources.
      String vrsltFileName = rptBuilder.BuildReportSync();
    }
    static void testReport1() {
      DataTable tbl = null;
      CXLReportDSConfigs v_dss = new CXLReportDSConfigs();
      v_dss.Add(new CXLReportDSConfig { 
        alias = "cdsRpt", 
        rangeName = "rngRpt",
        sql = "dbo.ant_get_org", 
        commandType = CommandType.StoredProcedure 
      });
      CParams prms = new CParams();
      prms.Add("@p0", 38);

      String vrsltFileName = CXLReport.BuildReportSync(new CXLReportConfig{
        title = "Заголовок отчета",
        subject = "Описание отчета",
        templateAdv =  @"d:\data\prjs\bioSys-8\Bio.Helpers\Bio.XLFRpt2\Bio.XLFRpt2.Test\bin\Debug\report100(rpt).xls",
        inPrms = prms,
        dataFactoryTypeName = "Bio.Helpers.XLFRpt2.DataFactory.MSSQL.CDataFactory, Bio.Helpers.XLFRpt2.MSSQLDataFactory",
        filenameFmt = "{$code}_{$now}",
        dss = v_dss,
        dbSession = null,
        //dbSession = new CRptDBSession(@"Data Source= EKP51\SQLEXPRESS; Initial Catalog=GIVCORG; Integrated Security=True; User ID=ayrat; Password=rtabh"),
        connStr = @"Data Source= EKP51\SQLEXPRESS; Initial Catalog=GIVCORG; Integrated Security=True; User ID=ayrat; Password=rtabh",
        debug = false
      });
    }

    static void testReport2() {
      DataTable tbl = new DataTable();
      tbl.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(Int32) });
      tbl.Columns.Add(new DataColumn { ColumnName = "ORGNAME", DataType = typeof(String) });
      tbl.Rows.Add(1, "org 1");
      //tbl.Rows.Add(1, "org 2");

      CXLReportDSConfigs v_dss = new CXLReportDSConfigs();
      v_dss.Add(new CXLReportDSConfig {
        alias = "cdsRpt",
        rangeName = "rngRpt",
        outerDataTable = tbl
      });
      CParams prms = new CParams();
      prms.Add("org_type", "p");

      String vrsltFileName = CXLReport.BuildReportSync(new CXLReportConfig {
        title = "Заголовок отчета",
        subject = "Описание отчета",
        templateAdv = @"report100(rpt).xls",
        inPrms = prms,
        filenameFmt = "{$code}_{$now}",
        dss = v_dss,
        debug = true
      });
    }

  }
}
