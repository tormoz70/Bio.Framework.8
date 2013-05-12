/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     12.05.2013 0:22:25                           */
/*==============================================================*/


alter table USRGRNT
   drop constraint GRANT$USR_FK;

alter table UGRANT
   drop primary key cascade;

alter table UGRANT
   drop constraint CKC_GRANT_UID_UGRANT;

alter table UGRANT
   drop constraint CKC_IS_SYS_UGRANT;

drop table "tmp_UGRANT" cascade constraints;

rename UGRANT to "tmp_UGRANT";

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

insert into UGRANT (GRANT_UID, ANAME, IS_SYS, ADESC)
select GRANT_UID, ANAME, IS_SYS, ADESC
from "tmp_UGRANT";

alter table USRGRNT
   add constraint GRANT$USR_FK foreign key (GRANT_UID)
      references UGRANT (GRANT_UID);

