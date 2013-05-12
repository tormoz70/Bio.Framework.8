/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     12.05.2013 13:59:40                          */
/*==============================================================*/


alter table USR
   drop constraint "Reference_12";

alter table USR
   add constraint WS$USR_FK foreign key (WORKSPACE_ID)
      references UWORKSPACE (WORKSPACE_ID);

