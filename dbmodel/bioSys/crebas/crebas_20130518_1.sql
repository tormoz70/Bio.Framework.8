/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     18.05.2013 23:45:05                          */
/*==============================================================*/


alter table USR
   drop constraint CKC_BLOCKED_USR;

alter table USR 
   drop column BLOCKED;

