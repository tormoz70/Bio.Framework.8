/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     12.05.2013 13:57:37                          */
/*==============================================================*/


alter table USR
   drop constraint ORG$USR_FK;

alter table USRGRNT
   drop constraint USR$GRANT_FK;

alter table USRLOCK
   drop constraint "Reference_11";

alter table USRLOG
   drop constraint USR$LOG_FK;

alter table USRRLE
   drop constraint USR$ROLE_FK;

alter table UWORKSPACE
   drop constraint USR$WS_FK;

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

drop index USR$WS_FK;

/*==============================================================*/
/* Table: USR                                                   */
/*==============================================================*/
create table USR  (
   USR_UID              VARCHAR2(32)                    not null
      constraint CKC_USR_UID_USR check (USR_UID = upper(USR_UID)),
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

--WARNING: The following insert order will fail because it cannot give value to mandatory columns
insert into USR (USR_UID, ORG_ID, WORKSPACE_ID, USR_NAME, USR_PWD, FIO_FAM, FIO_FNAME, FIO_SNAME, REG_DATE, BLOCKED, EMAIL_ADDR, USR_PHONE, CONFIRMED, GARBAGED, EXTINFO)
select USR_UID, ORG_ID, ?, USR_NAME, USR_PWD, FIO_FAM, FIO_FNAME, FIO_SNAME, REG_DATE, BLOCKED, EMAIL_ADDR, USR_PHONE, CONFIRMED, GARBAGED, EXTINFO
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

alter table UWORKSPACE add ADESC VARCHAR2(1000);

comment on column UWORKSPACE.ADESC is
'Описание пространства';

alter table UWORKSPACE
   drop constraint CKC_USR_UID_UWORKSPA;

alter table UWORKSPACE
   modify USR_UID null;

alter table UWORKSPACE
   add constraint CKC_USR_UID_UWORKSPA check (USR_UID is null or (USR_UID = upper(USR_UID)));

/*==============================================================*/
/* Index: USR$WS_FK                                             */
/*==============================================================*/
create index USR$WS_FK on UWORKSPACE (
   USR_UID ASC
)
tablespace BIOSYS_INDX;

alter table USR
   add constraint ORG$USR_FK foreign key (ORG_ID)
      references ORG (ORG_ID);

alter table USR
   add constraint "Reference_12" foreign key (WORKSPACE_ID)
      references UWORKSPACE (WORKSPACE_ID);

alter table USRGRNT
   add constraint USR$GRANT_FK foreign key (USR_UID)
      references USR (USR_UID);

alter table USRLOCK
   add constraint "Reference_11" foreign key (USR_UID)
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

