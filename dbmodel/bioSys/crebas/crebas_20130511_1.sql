/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     11.05.2013 21:33:47                          */
/*==============================================================*/


alter table USRIN$LOG
   drop constraint ADDR$LOG_FK;

drop index ADDR$LOG_FK;

alter table USRIN$LOG
   drop primary key cascade;

alter table USRIN$LOG
   drop constraint CKC_SESSION_ID_USRIN$LO;

drop table "tmp_USRIN$LOG" cascade constraints;

rename USRIN$LOG to "tmp_USRIN$LOG";

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

--WARNING: The following insert order will fail because it cannot give value to mandatory columns
insert into USRIN$LOG (REC_ID, REM_ADDR, USR_NAME, SESSION_ID, REM_CLIENT, ASTATUS, USRIN_DATE)
select ?, REM_ADDR, USR_NAME, SESSION_ID, REM_CLIENT, ASTATUS, USRIN_DATE
from "tmp_USRIN$LOG";

/*==============================================================*/
/* Index: ADDR$LOG_FK                                           */
/*==============================================================*/
create index ADDR$LOG_FK on USRIN$LOG (
   REM_ADDR ASC
)
tablespace BIOSYS_INDX;

alter table USRIN$LOG
   add constraint ADDR$LOG_FK foreign key (REM_ADDR)
      references RADDRSS (REM_ADDR);

