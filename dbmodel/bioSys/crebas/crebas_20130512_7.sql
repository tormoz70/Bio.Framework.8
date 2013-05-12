/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     12.05.2013 17:26:35                          */
/*==============================================================*/


alter table SMTP$LOG 
   rename column USR_NAME to USR_LOGIN;

alter table USRIN$LOG 
   rename column USR_NAME to USR_LOGIN;

