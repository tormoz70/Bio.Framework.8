/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     12.05.2013 14:27:06                          */
/*==============================================================*/


alter table ORG
   drop constraint WS$ORG_FK;

alter table USR
   drop constraint ORG$USR_FK;

alter table USR
   drop constraint WS$USR_FK;

alter table USRGRNT
   drop constraint GRANT$USR_FK;

alter table USRGRNT
   drop constraint USR$GRANT_FK;

alter table USRLOCK
   drop constraint USR$LCK_FK;

alter table USRLOG
   drop constraint USR$LOG_FK;

alter table USRRLE
   drop constraint ROLE$USR_FK;

alter table USRRLE
   drop constraint USR$ROLE_FK;

alter table UWORKSPACE
   drop constraint USR$WS_FK;

drop index WS$USR_FK;

drop index ORG$USR_FK;

drop index IX_USRS_GBRG;

alter table USR
   drop primary key cascade;

alter table USR
   drop constraint CKC_USR_UID_USR;

alter table USR
   drop constraint CKC_USR_NAME_USR;

alter table USR
   drop constraint CKC_FIO_FAM_USR;

alter table USR
   drop constraint CKC_FIO_FNAME_USR;

alter table USR
   drop constraint CKC_FIO_SNAME_USR;

alter table USR
   drop constraint CKC_BLOCKED_USR;

alter table USR
   drop constraint CKC_CONFIRMED_USR;

alter table USR
   drop constraint CKC_GARBAGED_USR;

drop table "tmp_USR" cascade constraints;

rename USR to "tmp_USR";

drop index GRANT$USR_FK;

drop index USR$GRANT_FK;

alter table USRGRNT
   drop primary key cascade;

alter table USRGRNT
   drop constraint CKC_GRANT_UID_USRGRNT;

alter table USRGRNT
   drop constraint CKC_USR_UID_USRGRNT;

drop table "tmp_USRGRNT" cascade constraints;

rename USRGRNT to "tmp_USRGRNT";

drop index USR$LCK_FK;

alter table USRLOCK
   drop primary key cascade;

alter table USRLOCK
   drop constraint CKC_USR_UID_USRLOCK;

alter table USRLOCK
   drop constraint CKC_LOCK_TYPE_USRLOCK;

alter table USRLOCK
   drop constraint CKC_DELETED_USRLOCK;

drop table "tmp_USRLOCK" cascade constraints;

rename USRLOCK to "tmp_USRLOCK";

drop index USR$LOG_FK;

alter table USRLOG
   drop primary key cascade;

alter table USRLOG
   drop constraint CKC_USR_UID_USRLOG;

alter table USRLOG
   drop constraint CKC_IOBJ_ID_USRLOG;

alter table USRLOG
   drop constraint CKC_IOBJ_MASTER_ID_USRLOG;

drop table "tmp_USRLOG" cascade constraints;

rename USRLOG to "tmp_USRLOG";

drop index USR$ROLE_FK;

drop index ROLE$USR_FK;

alter table USRRLE
   drop primary key cascade;

alter table USRRLE
   drop constraint CKC_ROLE_UID_USRRLE;

alter table USRRLE
   drop constraint CKC_USR_UID_USRRLE;

drop table "tmp_USRRLE" cascade constraints;

rename USRRLE to "tmp_USRRLE";

drop index USR$WS_FK;

alter table UWORKSPACE
   drop primary key cascade;

alter table UWORKSPACE
   drop constraint CKC_USR_UID_UWORKSPA;

drop table "tmp_UWORKSPACE" cascade constraints;

rename UWORKSPACE to "tmp_UWORKSPACE";

/*==============================================================*/
/* Table: USR                                                   */
/*==============================================================*/
create table USR  (
   USR_ID               NUMBER(18)                      not null,
   ORG_ID               NUMBER(18),
   WORKSPACE_ID         NUMBER(18)                      not null,
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
   constraint PK_USR primary key (USR_ID)
         using index tablespace BIOSYS_INDX
);

comment on table USR is
'Пользователи';

comment on column USR.USR_ID is
'ID пользователя';

comment on column USR.ORG_ID is
'ID подразделения';

comment on column USR.WORKSPACE_ID is
'ID пространства';

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

--WARNING: The following insert order will not restore columns: USR_UID
insert into USR (USR_ID, ORG_ID, WORKSPACE_ID, USR_NAME, USR_PWD, FIO_FAM, FIO_FNAME, FIO_SNAME, REG_DATE, BLOCKED, EMAIL_ADDR, USR_PHONE, CONFIRMED, GARBAGED, EXTINFO)
select ?, ORG_ID, WORKSPACE_ID, USR_NAME, USR_PWD, FIO_FAM, FIO_FNAME, FIO_SNAME, REG_DATE, BLOCKED, EMAIL_ADDR, USR_PHONE, CONFIRMED, GARBAGED, EXTINFO
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
/* Table: USRGRNT                                               */
/*==============================================================*/
create table USRGRNT  (
   GRANT_UID            VARCHAR2(64)                    not null
      constraint CKC_GRANT_UID_USRGRNT check (GRANT_UID = upper(GRANT_UID)),
   USR_ID               NUMBER(18)                      not null,
   constraint PK_USRGRNT primary key (GRANT_UID, USR_ID)
         using index tablespace BIOSYS_INDX
);

comment on table USRGRNT is
'Разрешение пользователя';

comment on column USRGRNT.GRANT_UID is
'UID разрешения';

comment on column USRGRNT.USR_ID is
'ID пользователя';

--WARNING: The following insert order will not restore columns: USR_UID
insert into USRGRNT (GRANT_UID, USR_ID)
select GRANT_UID, ?
from "tmp_USRGRNT";

/*==============================================================*/
/* Index: USR$GRANT_FK                                          */
/*==============================================================*/
create index USR$GRANT_FK on USRGRNT (
   USR_ID ASC
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
/* Table: USRLOCK                                               */
/*==============================================================*/
create table USRLOCK  (
   LOCK_ID              NUMBER(18)                      not null,
   USR_ID               NUMBER(18)                      not null,
   LOCK_TYPE            CHAR(1)                        default '0' not null
      constraint CKC_LOCK_TYPE_USRLOCK check (LOCK_TYPE in ('0','1')),
   CREATED              DATE                            not null,
   FROM_POINT           DATE                            not null,
   TO_POINT             DATE,
   COMMENTS             VARCHAR2(4000),
   DELETED              CHAR(1)                        default '0' not null
      constraint CKC_DELETED_USRLOCK check (DELETED in ('0','1')),
   constraint PK_USRLOCK primary key (LOCK_ID)
         using index tablespace BIOSYS_INDX
);

comment on table USRLOCK is
'Блокировка пользователя. Данные пользователи блокируются на указанный период';

comment on column USRLOCK.LOCK_ID is
'ID блокировки';

comment on column USRLOCK.USR_ID is
'ID пользователя';

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

--WARNING: The following insert order will not restore columns: USR_UID
insert into USRLOCK (LOCK_ID, USR_ID, LOCK_TYPE, CREATED, FROM_POINT, TO_POINT, COMMENTS, DELETED)
select LOCK_ID, ?, LOCK_TYPE, CREATED, FROM_POINT, TO_POINT, COMMENTS, DELETED
from "tmp_USRLOCK";

/*==============================================================*/
/* Index: USR$LCK_FK                                            */
/*==============================================================*/
create index USR$LCK_FK on USRLOCK (
   USR_ID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: USRLOG                                                */
/*==============================================================*/
create table USRLOG  (
   USRLOG_ID            NUMBER(18)                      not null,
   USR_ID               NUMBER(18)                      not null,
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

comment on column USRLOG.USR_ID is
'ID пользователя';

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

--WARNING: The following insert order will not restore columns: USR_UID
insert into USRLOG (USRLOG_ID, USR_ID, IOBJ_CD, IOBJ_ID, ACTION_TEXT, ACT_DATE, IOBJ_MASTER_CD, IOBJ_MASTER_ID)
select USRLOG_ID, ?, IOBJ_CD, IOBJ_ID, ACTION_TEXT, ACT_DATE, IOBJ_MASTER_CD, IOBJ_MASTER_ID
from "tmp_USRLOG";

/*==============================================================*/
/* Index: USR$LOG_FK                                            */
/*==============================================================*/
create index USR$LOG_FK on USRLOG (
   USR_ID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: USRRLE                                                */
/*==============================================================*/
create table USRRLE  (
   ROLE_UID             VARCHAR2(64)                    not null
      constraint CKC_ROLE_UID_USRRLE check (ROLE_UID = upper(ROLE_UID)),
   USR_ID               NUMBER(18)                      not null,
   constraint PK_USRRLE primary key (ROLE_UID, USR_ID)
         using index tablespace BIOSYS_INDX
);

comment on table USRRLE is
'Роль пользователя';

comment on column USRRLE.ROLE_UID is
'UID роли';

comment on column USRRLE.USR_ID is
'ID пользователя';

--WARNING: The following insert order will not restore columns: USR_UID
insert into USRRLE (ROLE_UID, USR_ID)
select ROLE_UID, ?
from "tmp_USRRLE";

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
   USR_ID ASC
)
tablespace BIOSYS_INDX;

/*==============================================================*/
/* Table: UWORKSPACE                                            */
/*==============================================================*/
create table UWORKSPACE  (
   WORKSPACE_ID         NUMBER(18)                      not null,
   USR_ID               NUMBER(18),
   ANAME                VARCHAR2(200)                   not null,
   ADESC                VARCHAR2(1000),
   constraint PK_UWORKSPACE primary key (WORKSPACE_ID)
         using index tablespace BIOSYS_INDX
);

comment on table UWORKSPACE is
'Пространство';

comment on column UWORKSPACE.WORKSPACE_ID is
'ID пространства';

comment on column UWORKSPACE.USR_ID is
'ID пользователя';

comment on column UWORKSPACE.ANAME is
'Имя пространства';

comment on column UWORKSPACE.ADESC is
'Описание пространства';

--WARNING: The following insert order will not restore columns: USR_UID
insert into UWORKSPACE (WORKSPACE_ID, ANAME, ADESC)
select WORKSPACE_ID, ANAME, ADESC
from "tmp_UWORKSPACE";

/*==============================================================*/
/* Index: USR$WS_FK                                             */
/*==============================================================*/
create index USR$WS_FK on UWORKSPACE (
   USR_ID ASC
)
tablespace BIOSYS_INDX;

alter table ORG
   add constraint WS$ORG_FK foreign key (WORKSPACE_ID)
      references UWORKSPACE (WORKSPACE_ID);

alter table USR
   add constraint ORG$USR_FK foreign key (ORG_ID)
      references ORG (ORG_ID);

alter table USR
   add constraint WS$USR_FK foreign key (WORKSPACE_ID)
      references UWORKSPACE (WORKSPACE_ID);

alter table USRGRNT
   add constraint GRANT$USR_FK foreign key (GRANT_UID)
      references UGRANT (GRANT_UID);

alter table USRGRNT
   add constraint USR$GRANT_FK foreign key (USR_ID)
      references USR (USR_ID);

alter table USRLOCK
   add constraint USR$LCK_FK foreign key (USR_ID)
      references USR (USR_ID);

alter table USRLOG
   add constraint USR$LOG_FK foreign key (USR_ID)
      references USR (USR_ID);

alter table USRRLE
   add constraint ROLE$USR_FK foreign key (ROLE_UID)
      references UROLE (ROLE_UID);

alter table USRRLE
   add constraint USR$ROLE_FK foreign key (USR_ID)
      references USR (USR_ID);

alter table UWORKSPACE
   add constraint USR$WS_FK foreign key (USR_ID)
      references USR (USR_ID);

