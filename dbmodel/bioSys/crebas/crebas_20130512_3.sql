/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     12.05.2013 14:14:31                          */
/*==============================================================*/


alter table USRLOCK
   drop constraint "Reference_11";

alter table USRLOCK
   drop primary key cascade;

/*==============================================================*/
/* Index: WS$USR_FK                                             */
/*==============================================================*/
create index WS$USR_FK on USR (
   WORKSPACE_ID ASC
)
tablespace BIOSYS_INDX;

alter table USRLOCK
   add constraint PK_USRLOCK primary key (LOCK_ID)
      using index tablespace BIOSYS_INDX;

/*==============================================================*/
/* Index: USR$LCK_FK                                            */
/*==============================================================*/
create index USR$LCK_FK on USRLOCK (
   USR_UID ASC
)
tablespace BIOSYS_INDX;

alter table USRLOCK
   add constraint USR$LCK_FK foreign key (USR_UID)
      references USR (USR_UID);

