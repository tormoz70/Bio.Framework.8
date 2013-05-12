/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     11.05.2013 22:49:50                          */
/*==============================================================*/


alter table ORG 
   rename column ODEP_ID to ORG_ID;

alter table ORG 
   rename column PRNT_ODEP_ID to PRNT_ORG_ID;

