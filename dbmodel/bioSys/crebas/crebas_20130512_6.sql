/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     12.05.2013 17:24:42                          */
/*==============================================================*/


alter table USR
   drop constraint ORG$USR_FK;

alter table USR
   drop constraint WS$USR_FK;

alter table USRGRNT
   drop constraint USR$GRANT_FK;

alter table USRIN$LOG
   drop constraint ADDR$LOG_FK;

alter table USRLOCK
   drop constraint USR$LCK_FK;

alter table USRLOG
   drop constraint USR$LOG_FK;

alter table USRRLE
   drop constraint USR$ROLE_FK;

alter table UWORKSPACE
   drop constraint USR$WS_FK;

alter table SMTP$LOG
   drop primary key cascade;

drop table "tmp_SMTP$LOG" cascade constraints;

/*==============================================================*/
/* Table: "tmp_SMTP$LOG"                                        */
/*==============================================================*/
create table "tmp_SMTP$LOG"  (
   ERR_UID              VARCHAR2(32)                    not null,
   CRE_DATE             DATE                            not null,
   SRV_ADDR             VARCHAR2(100)                   not null,
   PORT_NUM             NUMBER(6)                       not null,
   USR_NAME             VARCHAR2(64),
   USR_PWD              VARCHAR2(32),
   ADDR_FROM            VARCHAR2(100)                   not null,
   ADDR_TO              VARCHAR2(100)                   not null,
   MSG_SUBJ             VARCHAR2(1000),
   ERROR_TEXT           CLOB                            not null
);

comment on table "tmp_SMTP$LOG" is
'Лог ошибок отправки сообщений по SMTP';

comment on column "tmp_SMTP$LOG".ERR_UID is
'UID ошибки';

comment on column "tmp_SMTP$LOG".CRE_DATE is
'Дата/время';

comment on column "tmp_SMTP$LOG".SRV_ADDR is
'Сервер';

comment on column "tmp_SMTP$LOG".PORT_NUM is
'Порт';

comment on column "tmp_SMTP$LOG".USR_NAME is
'Пользователь';

comment on column "tmp_SMTP$LOG".USR_PWD is
'Пароль';

comment on column "tmp_SMTP$LOG".ADDR_FROM is
'Адрес отправителя';

comment on column "tmp_SMTP$LOG".ADDR_TO is
'Адрес получателя';

comment on column "tmp_SMTP$LOG".MSG_SUBJ is
'Тема сообщения';

comment on column "tmp_SMTP$LOG".ERROR_TEXT is
'Текст ошибки';

insert into "tmp_SMTP$LOG" (ERR_UID, CRE_DATE, SRV_ADDR, PORT_NUM, USR_NAME, USR_PWD, ADDR_FROM, ADDR_TO, MSG_SUBJ, ERROR_TEXT)
select ERR_UID, CRE_DATE, SRV_ADDR, PORT_NUM, USR_NAME, USR_PWD, ADDR_FROM, ADDR_TO, MSG_SUBJ, ERROR_TEXT
from SMTP$LOG;

drop table SMTP$LOG cascade constraints;

drop index WS$USR_FK;

drop index ORG$USR_FK;

drop index IX_USRS_GBRG;

alter table USR
   drop primary key cascade;

drop table "tmp_USR" cascade constraints;

/*==============================================================*/
/* Table: "tmp_USR"                                             */
/*==============================================================*/
create table "tmp_USR"  (
   USR_UID              VARCHAR2(32)                    not null,
   ORG_ID               NUMBER(18),
   WORKSPACE_ID         NUMBER(18)                      not null,
   USR_NAME             VARCHAR2(64)                    not null,
   USR_PWD              VARCHAR2(32)                    not null,
   FIO_FAM              VARCHAR2(100)                   not null,
   FIO_FNAME            VARCHAR2(100)                   not null,
   FIO_SNAME            VARCHAR2(100)                   not null,
   REG_DATE             DATE                            not null,
   BLOCKED              CHAR(1)                        default '0' not null,
   EMAIL_ADDR           VARCHAR2(100)                   not null,
   USR_PHONE            VARCHAR2(12),
   CONFIRMED            CHAR(1)                        default '0',
   GARBAGED             CHAR(1)                        default '0' not null,
   EXTINFO              VARCHAR2(1000)
);

comment on table "tmp_USR" is
'Пользователи';

comment on column "tmp_USR".USR_UID is
'UID пользователя';

comment on column "tmp_USR".ORG_ID is
'ID подразделения';

comment on column "tmp_USR".WORKSPACE_ID is
'ID пространства';

comment on column "tmp_USR".USR_NAME is
'Логин';

comment on column "tmp_USR".USR_PWD is
'Пароль';

comment on column "tmp_USR".FIO_FAM is
'Фамилия';

comment on column "tmp_USR".FIO_FNAME is
'Имя';

comment on column "tmp_USR".FIO_SNAME is
'Отчество';

comment on column "tmp_USR".REG_DATE is
'Дата регистрации';

comment on column "tmp_USR".BLOCKED is
'Заблокирован (0-False; 1-True)';

comment on column "tmp_USR".EMAIL_ADDR is
'e-mail';

comment on column "tmp_USR".USR_PHONE is
'Телефон';

comment on column "tmp_USR".CONFIRMED is
'Подтверждение (0-False; 1-True)';

comment on column "tmp_USR".GARBAGED is
'В мусор (0-False; 1-True)';

comment on column "tmp_USR".EXTINFO is
'Дополнительная информация';

insert into "tmp_USR" (USR_UID, ORG_ID, WORKSPACE_ID, USR_NAME, USR_PWD, FIO_FAM, FIO_FNAME, FIO_SNAME, REG_DATE, BLOCKED, EMAIL_ADDR, USR_PHONE, CONFIRMED, GARBAGED, EXTINFO)
select USR_UID, ORG_ID, WORKSPACE_ID, USR_NAME, USR_PWD, FIO_FAM, FIO_FNAME, FIO_SNAME, REG_DATE, BLOCKED, EMAIL_ADDR, USR_PHONE, CONFIRMED, GARBAGED, EXTINFO
from USR;

drop table USR cascade constraints;

drop index ADDR$LOG_FK;

alter table USRIN$LOG
   drop primary key cascade;

drop table "tmp_USRIN$LOG" cascade constraints;

/*==============================================================*/
/* Table: "tmp_USRIN$LOG"                                       */
/*==============================================================*/
create table "tmp_USRIN$LOG"  (
   REC_ID               NUMBER(18)                      not null,
   REM_ADDR             VARCHAR2(32)                    not null,
   USR_NAME             VARCHAR2(64)                    not null,
   SESSION_ID           VARCHAR2(32)                    not null,
   REM_CLIENT           VARCHAR2(1000)                  not null,
   ASTATUS              VARCHAR2(200)                   not null,
   USRIN_DATE           DATE                            not null
);

comment on table "tmp_USRIN$LOG" is
'Лог входов в систему';

comment on column "tmp_USRIN$LOG".REC_ID is
'ID записи';

comment on column "tmp_USRIN$LOG".REM_ADDR is
'Адрес пользователя';

comment on column "tmp_USRIN$LOG".USR_NAME is
'Логин';

comment on column "tmp_USRIN$LOG".SESSION_ID is
'ID Сессии';

comment on column "tmp_USRIN$LOG".REM_CLIENT is
'Клиент пользователя';

comment on column "tmp_USRIN$LOG".ASTATUS is
'Статус';

comment on column "tmp_USRIN$LOG".USRIN_DATE is
'Дата входа';

insert into "tmp_USRIN$LOG" (REC_ID, REM_ADDR, USR_NAME, SESSION_ID, REM_CLIENT, ASTATUS, USRIN_DATE)
select REC_ID, REM_ADDR, USR_NAME, SESSION_ID, REM_CLIENT, ASTATUS, USRIN_DATE
from USRIN$LOG;

drop table USRIN$LOG cascade constraints;

/*==============================================================*/
/* Domain: USR_LOGIN_T                                          */
/*==============================================================*/
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

insert into SMTP$LOG (ERR_UID, CRE_DATE, SRV_ADDR, PORT_NUM, USR_NAME, USR_PWD, ADDR_FROM, ADDR_TO, MSG_SUBJ, ERROR_TEXT)
select ERR_UID, CRE_DATE, SRV_ADDR, PORT_NUM, USR_NAME, USR_PWD, ADDR_FROM, ADDR_TO, MSG_SUBJ, ERROR_TEXT
from "tmp_SMTP$LOG";

/*==============================================================*/
/* Table: USR                                                   */
/*==============================================================*/
create table USR  (
   USR_UID              VARCHAR2(32)                    not null
      constraint CKC_USR_UID_USR check (USR_UID = upper(USR_UID)),
   ORG_ID               NUMBER(18),
   WORKSPACE_ID         NUMBER(18)                      not null,
   USR_LOGIN            VARCHAR2(64)                    not null
      constraint CKC_USR_LOGIN_USR check (USR_LOGIN = lower(USR_LOGIN)),
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

comment on column USR.WORKSPACE_ID is
'ID пространства';

comment on column USR.USR_LOGIN is
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

insert into USR (USR_UID, ORG_ID, WORKSPACE_ID, USR_LOGIN, USR_PWD, FIO_FAM, FIO_FNAME, FIO_SNAME, REG_DATE, BLOCKED, EMAIL_ADDR, USR_PHONE, CONFIRMED, GARBAGED, EXTINFO)
select USR_UID, ORG_ID, WORKSPACE_ID, USR_NAME, USR_PWD, FIO_FAM, FIO_FNAME, FIO_SNAME, REG_DATE, BLOCKED, EMAIL_ADDR, USR_PHONE, CONFIRMED, GARBAGED, EXTINFO
from "tmp_USR";

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
/* Index: WS$USR_FK                                             */
/*==============================================================*/
create index WS$USR_FK on USR (
   WORKSPACE_ID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: USRIN$LOG                                             */
/*==============================================================*/
create table USRIN$LOG  (
   REC_ID               NUMBER(18)                      not null,
   REM_ADDR             VARCHAR2(32)                    not null,
   USR_NAME             VARCHAR2(64)                    not null,
   SESSION_ID           VARCHAR2(32)                    not null
      constraint CKC_SESSION_ID_USRIN$LO check (SESSION_ID = upper(SESSION_ID)),
   REM_CLIENT           VARCHAR2(1000)                  not null,
   ASTATUS              VARCHAR2(200)                   not null,
   USRIN_DATE           DATE                            not null,
   constraint PK_USRIN$LOG primary key (REC_ID)
         using index tablespace BIOSYS_INDX
);

comment on table USRIN$LOG is
'Лог входов в систему';

comment on column USRIN$LOG.REC_ID is
'ID записи';

comment on column USRIN$LOG.REM_ADDR is
'Адрес пользователя';

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

insert into USRIN$LOG (REC_ID, REM_ADDR, USR_NAME, SESSION_ID, REM_CLIENT, ASTATUS, USRIN_DATE)
select REC_ID, REM_ADDR, USR_NAME, SESSION_ID, REM_CLIENT, ASTATUS, USRIN_DATE
from "tmp_USRIN$LOG";

/*==============================================================*/
/* Index: ADDR$LOG_FK                                           */
/*==============================================================*/
create index ADDR$LOG_FK on USRIN$LOG (
   REM_ADDR ASC
)
tablespace BIOSYS_INDX;

alter table USR
   add constraint ORG$USR_FK foreign key (ORG_ID)
      references ORG (ORG_ID);

alter table USR
   add constraint WS$USR_FK foreign key (WORKSPACE_ID)
      references UWORKSPACE (WORKSPACE_ID);

alter table USRGRNT
   add constraint USR$GRANT_FK foreign key (USR_UID)
      references USR (USR_UID);

alter table USRIN$LOG
   add constraint ADDR$LOG_FK foreign key (REM_ADDR)
      references RADDRSS (REM_ADDR);

alter table USRLOCK
   add constraint USR$LCK_FK foreign key (USR_UID)
      references USR (USR_UID);

alter table USRLOG
   add constraint USR$LOG_FK foreign key (USR_UID)
      references USR (USR_UID);

alter table USRRLE
   add constraint USR$ROLE_FK foreign key (USR_UID)
      references USR (USR_UID);

alter table UWORKSPACE
   add constraint USR$WS_FK foreign key (USR_UID)
      references USR (USR_UID);

