/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     11.05.2013 20:59:59                          */
/*==============================================================*/


alter table ORG
   drop constraint ORG$ORG_FK;

alter table ORG
   drop constraint WS$ORG_FK;

alter table USR
   drop constraint ORG$USR_FK;

alter table USRGRNT
   drop constraint GRANT$USR_FK;

alter table USRGRNT
   drop constraint USR$GRANT_FK;

alter table USRIN$LOG
   drop constraint ADDR$LOG_FK;

alter table USRLOCK
   drop constraint "Reference_11";

alter table USRLOG
   drop constraint USR$LOG_FK;

alter table USRRLE
   drop constraint ROLE$USR_FK;

alter table USRRLE
   drop constraint USR$ROLE_FK;

alter table UWORKSPACE
   drop constraint USR$WS_FK;

drop index WS$ORG_FK;

drop index ORG$ORG_FK;

drop table ORG cascade constraints;

drop table RADDRSS cascade constraints;

drop table SMTP$LOG cascade constraints;

drop table UGRANT cascade constraints;

drop table UROLE cascade constraints;

drop index ORG$USR_FK;

drop index IX_USRS_GBRG;

drop table USR cascade constraints;

drop index GRANT$USR_FK;

drop index USR$GRANT_FK;

drop table USRGRNT cascade constraints;

drop index ADDR$LOG_FK;

drop table USRIN$LOG cascade constraints;

drop table USRLOCK cascade constraints;

drop index USR$LOG_FK;

drop table USRLOG cascade constraints;

drop index USR$ROLE_FK;

drop index ROLE$USR_FK;

drop table USRRLE cascade constraints;

drop index USR$WS_FK;

drop table UWORKSPACE cascade constraints;

/*==============================================================*/
/* Table: ORG                                                   */
/*==============================================================*/
create table ORG  (
   ODEP_ID              NUMBER(18)                      not null,
   PRNT_ODEP_ID         NUMBER(18),
   WORKSPACE_ID         NUMBER(18)                      not null,
   ANAME                VARCHAR2(500)                   not null,
   ADESC                VARCHAR2(1000),
   constraint PK_ORG primary key (ODEP_ID)
         using index tablespace BIOSYS_INDX
);

comment on table ORG is
'Подразделение';

comment on column ORG.ODEP_ID is
'ID подразделения';

comment on column ORG.PRNT_ODEP_ID is
'ID подразделения-родителя';

comment on column ORG.WORKSPACE_ID is
'ID пространства';

comment on column ORG.ANAME is
'Название подразделения';

comment on column ORG.ADESC is
'Описание подразделения';

/*==============================================================*/
/* Index: ORG$ORG_FK                                            */
/*==============================================================*/
create index ORG$ORG_FK on ORG (
   PRNT_ODEP_ID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Index: WS$ORG_FK                                             */
/*==============================================================*/
create index WS$ORG_FK on ORG (
   WORKSPACE_ID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: RADDRSS                                               */
/*==============================================================*/
create table RADDRSS  (
   REM_ADDR             VARCHAR2(32)                    not null,
   REM_HOST             VARCHAR2(100)                   not null,
   ADDR_DESC            VARCHAR2(1000),
   constraint PK_RADDRSS primary key (REM_ADDR)
         using index tablespace BIOSYS_INDX
);

comment on table RADDRSS is
'Справочник адресов';

comment on column RADDRSS.REM_ADDR is
'Адрес пользователя';

comment on column RADDRSS.REM_HOST is
'Имя хоста пользователя';

comment on column RADDRSS.ADDR_DESC is
'Описание';

/*==============================================================*/
/* Table: SMTP$LOG                                              */
/*==============================================================*/
create table SMTP$LOG  (
   ERR_UID              VARCHAR2(32)                    not null
      constraint CKC_ERR_UID_SMTP$LOG check (ERR_UID = upper(ERR_UID)),
   CRE_DATE             DATE                            not null,
   SRV_ADDR             VARCHAR2(100)                   not null,
   PORT_NUM             NUMBER(6)                       not null,
   USR_NAME             VARCHAR2(64),
   USR_PWD              VARCHAR2(32),
   ADDR_FROM            VARCHAR2(100)                   not null,
   ADDR_TO              VARCHAR2(100)                   not null,
   MSG_SUBJ             VARCHAR2(1000),
   ERROR_TEXT           CLOB                            not null,
   constraint PK_SMTP$LOG primary key (ERR_UID)
         using index tablespace BIOSYS_INDX
);

comment on table SMTP$LOG is
'Лог ошибок отправки сообщений по SMTP';

comment on column SMTP$LOG.ERR_UID is
'UID ошибки';

comment on column SMTP$LOG.CRE_DATE is
'Дата/время';

comment on column SMTP$LOG.SRV_ADDR is
'Сервер';

comment on column SMTP$LOG.PORT_NUM is
'Порт';

comment on column SMTP$LOG.USR_NAME is
'Пользователь';

comment on column SMTP$LOG.USR_PWD is
'Пароль';

comment on column SMTP$LOG.ADDR_FROM is
'Адрес отправителя';

comment on column SMTP$LOG.ADDR_TO is
'Адрес получателя';

comment on column SMTP$LOG.MSG_SUBJ is
'Тема сообщения';

comment on column SMTP$LOG.ERROR_TEXT is
'Текст ошибки';

/*==============================================================*/
/* Table: UGRANT                                                */
/*==============================================================*/
create table UGRANT  (
   GRANT_UID            VARCHAR2(64)                    not null
      constraint CKC_GRANT_UID_UGRANT check (GRANT_UID = upper(GRANT_UID)),
   ANAME                VARCHAR2(200)                   not null,
   IS_SYS               CHAR(1)                        default '0' not null
      constraint CKC_IS_SYS_UGRANT check (IS_SYS in ('0','1')),
   ADESC                VARCHAR2(1000),
   constraint PK_UGRANT primary key (GRANT_UID)
         using index tablespace BIOSYS_INDX
);

comment on table UGRANT is
'Разрешение';

comment on column UGRANT.GRANT_UID is
'UID разрешения';

comment on column UGRANT.ANAME is
'Имя разрешения';

comment on column UGRANT.IS_SYS is
'Системное разрешения (0-False; 1-True)';

comment on column UGRANT.ADESC is
'Описание роли';

/*==============================================================*/
/* Table: UROLE                                                 */
/*==============================================================*/
create table UROLE  (
   ROLE_UID             VARCHAR2(64)                    not null
      constraint CKC_ROLE_UID_UROLE check (ROLE_UID = upper(ROLE_UID)),
   ANAME                VARCHAR2(200)                   not null,
   IS_SYS               CHAR(1)                        default '0' not null
      constraint CKC_IS_SYS_UROLE check (IS_SYS in ('0','1')),
   ADESC                VARCHAR2(1000),
   constraint PK_UROLE primary key (ROLE_UID)
         using index tablespace BIOSYS_INDX
);

comment on table UROLE is
'Роль';

comment on column UROLE.ROLE_UID is
'UID роли';

comment on column UROLE.ANAME is
'Имя роли';

comment on column UROLE.IS_SYS is
'Системная роль (0-False; 1-True)';

comment on column UROLE.ADESC is
'Описание роли';

/*==============================================================*/
/* Table: USR                                                   */
/*==============================================================*/
create table USR  (
   USR_UID              VARCHAR2(32)                    not null
      constraint CKC_USR_UID_USR check (USR_UID = upper(USR_UID)),
   ORG_ID               NUMBER(18),
   USR_NAME             VARCHAR2(64)                    not null
      constraint CKC_USR_NAME_USR check (USR_NAME = upper(USR_NAME)),
   USR_PWD              VARCHAR2(32)                    not null,
   FIO_FAM              VARCHAR2(100)                   not null
      constraint CKC_FIO_FAM_USR check (FIO_FAM = upper(FIO_FAM)),
   FIO_FNAME            VARCHAR2(100)                   not null
      constraint CKC_FIO_FNAME_USR check (FIO_FNAME = upper(FIO_FNAME)),
   FIO_SNAME            VARCHAR2(100)                   not null
      constraint CKC_FIO_SNAME_USR check (FIO_SNAME = upper(FIO_SNAME)),
   REG_DATE             DATE                            not null,
   BLOCKED              CHAR(1)                        default '0' not null
      constraint CKC_BLOCKED_USR check (BLOCKED in ('0','1')),
   EMAIL_ADDR           VARCHAR2(100)                   not null,
   USR_PHONE            VARCHAR2(12),
   CONFIRMED            CHAR(1)                        default '0'
      constraint CKC_CONFIRMED_USR check (CONFIRMED is null or (CONFIRMED in ('0','1'))),
   GARBAGED             CHAR(1)                        default '0' not null
      constraint CKC_GARBAGED_USR check (GARBAGED in ('0','1')),
   EXTINFO              VARCHAR2(1000),
   constraint PK_USR primary key (USR_UID)
         using index tablespace BIOSYS_INDX
);

comment on table USR is
'Пользователи';

comment on column USR.USR_UID is
'UID пользователя';

comment on column USR.ORG_ID is
'ID подразделения';

comment on column USR.USR_NAME is
'Логин';

comment on column USR.USR_PWD is
'Пароль';

comment on column USR.FIO_FAM is
'Фамилия';

comment on column USR.FIO_FNAME is
'Имя';

comment on column USR.FIO_SNAME is
'Отчество';

comment on column USR.REG_DATE is
'Дата регистрации';

comment on column USR.BLOCKED is
'Заблокирован (0-False; 1-True)';

comment on column USR.EMAIL_ADDR is
'e-mail';

comment on column USR.USR_PHONE is
'Телефон';

comment on column USR.CONFIRMED is
'Подтверждение (0-False; 1-True)';

comment on column USR.GARBAGED is
'В мусор (0-False; 1-True)';

comment on column USR.EXTINFO is
'Дополнительная информация';

/*==============================================================*/
/* Index: IX_USRS_GBRG                                          */
/*==============================================================*/
create index IX_USRS_GBRG on USR (
   GARBAGED ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Index: ORG$USR_FK                                            */
/*==============================================================*/
create index ORG$USR_FK on USR (
   ORG_ID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: USRGRNT                                               */
/*==============================================================*/
create table USRGRNT  (
   GRANT_UID            VARCHAR2(64)                    not null
      constraint CKC_GRANT_UID_USRGRNT check (GRANT_UID = upper(GRANT_UID)),
   USR_UID              VARCHAR2(32)                    not null
      constraint CKC_USR_UID_USRGRNT check (USR_UID = upper(USR_UID)),
   constraint PK_USRGRNT primary key (GRANT_UID, USR_UID)
         using index tablespace BIOSYS_INDX
);

comment on table USRGRNT is
'Разрешение пользователя';

comment on column USRGRNT.GRANT_UID is
'UID разрешения';

comment on column USRGRNT.USR_UID is
'UID пользователя';

/*==============================================================*/
/* Index: USR$GRANT_FK                                          */
/*==============================================================*/
create index USR$GRANT_FK on USRGRNT (
   USR_UID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Index: GRANT$USR_FK                                          */
/*==============================================================*/
create index GRANT$USR_FK on USRGRNT (
   GRANT_UID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: USRIN$LOG                                             */
/*==============================================================*/
create table USRIN$LOG  (
   REM_ADDR             VARCHAR2(32)                    not null,
   USRIN_NUM            NUMBER(18)                      not null,
   USR_NAME             VARCHAR2(64)                    not null,
   SESSION_ID           VARCHAR2(32)                    not null
      constraint CKC_SESSION_ID_USRIN$LO check (SESSION_ID = upper(SESSION_ID)),
   REM_CLIENT           VARCHAR2(1000)                  not null,
   ASTATUS              VARCHAR2(200)                   not null,
   USRIN_DATE           DATE                            not null,
   constraint PK_USRIN$LOG primary key (REM_ADDR, USRIN_NUM)
         using index tablespace BIOSYS_INDX
);

comment on table USRIN$LOG is
'Лог входов в систему';

comment on column USRIN$LOG.REM_ADDR is
'Адрес пользователя';

comment on column USRIN$LOG.USRIN_NUM is
'Номер входа';

comment on column USRIN$LOG.USR_NAME is
'Логин';

comment on column USRIN$LOG.SESSION_ID is
'ID Сессии';

comment on column USRIN$LOG.REM_CLIENT is
'Клиент пользователя';

comment on column USRIN$LOG.ASTATUS is
'Статус';

comment on column USRIN$LOG.USRIN_DATE is
'Дата входа';

/*==============================================================*/
/* Index: ADDR$LOG_FK                                           */
/*==============================================================*/
create index ADDR$LOG_FK on USRIN$LOG (
   REM_ADDR ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: USRLOCK                                               */
/*==============================================================*/
create table USRLOCK  (
   LOCK_ID              NUMBER(18)                      not null,
   USR_UID              VARCHAR2(32)                    not null
      constraint CKC_USR_UID_USRLOCK check (USR_UID = upper(USR_UID)),
   LOCK_TYPE            CHAR(1)                        default '0' not null
      constraint CKC_LOCK_TYPE_USRLOCK check (LOCK_TYPE in ('0','1')),
   CREATED              DATE                            not null,
   FROM_POINT           DATE                            not null,
   TO_POINT             DATE,
   COMMENTS             VARCHAR2(4000),
   DELETED              CHAR(1)                        default '0' not null
      constraint CKC_DELETED_USRLOCK check (DELETED in ('0','1')),
   constraint PK_USRLOCK primary key (LOCK_ID)
);

comment on table USRLOCK is
'Блокировка пользователя. Данные пользователи блокируются на указанный период';

comment on column USRLOCK.LOCK_ID is
'ID блокировки';

comment on column USRLOCK.USR_UID is
'UID пользователя';

comment on column USRLOCK.LOCK_TYPE is
'Тип блокировки (0-ручная; 1-автоматическая)';

comment on column USRLOCK.CREATED is
'Дата/Время создания';

comment on column USRLOCK.FROM_POINT is
'Дата/Время начала блокировки';

comment on column USRLOCK.TO_POINT is
'Дата/Время окончания блокировки';

comment on column USRLOCK.COMMENTS is
'Комментарии';

comment on column USRLOCK.DELETED is
'Блокировка удалена (0-False; 1-True)';

/*==============================================================*/
/* Table: USRLOG                                                */
/*==============================================================*/
create table USRLOG  (
   USRLOG_ID            NUMBER(18)                      not null,
   USR_UID              VARCHAR2(32)                    not null
      constraint CKC_USR_UID_USRLOG check (USR_UID = upper(USR_UID)),
   IOBJ_CD              VARCHAR2(30)                    not null,
   IOBJ_ID              VARCHAR2(32)                    not null
      constraint CKC_IOBJ_ID_USRLOG check (IOBJ_ID = upper(IOBJ_ID)),
   ACTION_TEXT          CLOB                            not null,
   ACT_DATE             DATE                            not null,
   IOBJ_MASTER_CD       VARCHAR2(30),
   IOBJ_MASTER_ID       VARCHAR2(32)                   
      constraint CKC_IOBJ_MASTER_ID_USRLOG check (IOBJ_MASTER_ID is null or (IOBJ_MASTER_ID = upper(IOBJ_MASTER_ID))),
   constraint PK_USRLOG primary key (USRLOG_ID)
         using index tablespace BIOSYS_INDX
);

comment on table USRLOG is
'Протокол действий пользователя';

comment on column USRLOG.USRLOG_ID is
'ID записи';

comment on column USRLOG.USR_UID is
'UID пользователя';

comment on column USRLOG.IOBJ_CD is
'Код измененного объекта (Имя таблицы)';

comment on column USRLOG.IOBJ_ID is
'ID объекта';

comment on column USRLOG.ACTION_TEXT is
'Описание действия';

comment on column USRLOG.ACT_DATE is
'Дата и время действия';

comment on column USRLOG.IOBJ_MASTER_CD is
'Код измененного master-объекта (Имя таблицы)';

comment on column USRLOG.IOBJ_MASTER_ID is
'ID master-объекта';

/*==============================================================*/
/* Index: USR$LOG_FK                                            */
/*==============================================================*/
create index USR$LOG_FK on USRLOG (
   USR_UID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: USRRLE                                                */
/*==============================================================*/
create table USRRLE  (
   ROLE_UID             VARCHAR2(64)                    not null
      constraint CKC_ROLE_UID_USRRLE check (ROLE_UID = upper(ROLE_UID)),
   USR_UID              VARCHAR2(32)                    not null
      constraint CKC_USR_UID_USRRLE check (USR_UID = upper(USR_UID)),
   constraint PK_USRRLE primary key (ROLE_UID, USR_UID)
         using index tablespace BIOSYS_INDX
);

comment on table USRRLE is
'Роль пользователя';

comment on column USRRLE.ROLE_UID is
'UID роли';

comment on column USRRLE.USR_UID is
'UID пользователя';

/*==============================================================*/
/* Index: ROLE$USR_FK                                           */
/*==============================================================*/
create index ROLE$USR_FK on USRRLE (
   ROLE_UID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Index: USR$ROLE_FK                                           */
/*==============================================================*/
create index USR$ROLE_FK on USRRLE (
   USR_UID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: UWORKSPACE                                            */
/*==============================================================*/
create table UWORKSPACE  (
   WORKSPACE_ID         NUMBER(18)                      not null,
   USR_UID              VARCHAR2(32)                    not null
      constraint CKC_USR_UID_UWORKSPA check (USR_UID = upper(USR_UID)),
   ANAME                VARCHAR2(200)                   not null,
   constraint PK_UWORKSPACE primary key (WORKSPACE_ID)
         using index tablespace BIOSYS_INDX
);

comment on table UWORKSPACE is
'Пространство';

comment on column UWORKSPACE.WORKSPACE_ID is
'ID пространства';

comment on column UWORKSPACE.USR_UID is
'UID пользователя';

comment on column UWORKSPACE.ANAME is
'Имя пространства';

/*==============================================================*/
/* Index: USR$WS_FK                                             */
/*==============================================================*/
create index USR$WS_FK on UWORKSPACE (
   USR_UID ASC
)
tablespace BIOSYS_INDX;

alter table ORG
   add constraint ORG$ORG_FK foreign key (PRNT_ODEP_ID)
      references ORG (ODEP_ID);

alter table ORG
   add constraint WS$ORG_FK foreign key (WORKSPACE_ID)
      references UWORKSPACE (WORKSPACE_ID);

alter table USR
   add constraint ORG$USR_FK foreign key (ORG_ID)
      references ORG (ODEP_ID);

alter table USRGRNT
   add constraint GRANT$USR_FK foreign key (GRANT_UID)
      references UGRANT (GRANT_UID);

alter table USRGRNT
   add constraint USR$GRANT_FK foreign key (USR_UID)
      references USR (USR_UID);

alter table USRIN$LOG
   add constraint ADDR$LOG_FK foreign key (REM_ADDR)
      references RADDRSS (REM_ADDR);

alter table USRLOCK
   add constraint "Reference_11" foreign key (USR_UID)
      references USR (USR_UID);

alter table USRLOG
   add constraint USR$LOG_FK foreign key (USR_UID)
      references USR (USR_UID);

alter table USRRLE
   add constraint ROLE$USR_FK foreign key (ROLE_UID)
      references UROLE (ROLE_UID);

alter table USRRLE
   add constraint USR$ROLE_FK foreign key (USR_UID)
      references USR (USR_UID);

alter table UWORKSPACE
   add constraint USR$WS_FK foreign key (USR_UID)
      references USR (USR_UID);

