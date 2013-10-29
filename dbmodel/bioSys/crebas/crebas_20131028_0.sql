/*==============================================================*/
/* DBMS name:      ORACLE Version 10gR2                         */
/* Created on:     28.10.2013 15:58:28                          */
/*==============================================================*/


ALTER TABLE BIOSYS.ORG
   DROP CONSTRAINT ORG$ORG_FK;

ALTER TABLE BIOSYS.ORG
   DROP CONSTRAINT WS$ORG_FK;

ALTER TABLE BIOSYS.USR
   DROP CONSTRAINT ORG$USR_FK;

ALTER TABLE BIOSYS.USR
   DROP CONSTRAINT WS$USR_FK;

ALTER TABLE BIOSYS.USRGRNT
   DROP CONSTRAINT GRANT$USR_FK;

ALTER TABLE BIOSYS.USRGRNT
   DROP CONSTRAINT USR$GRANT_FK;

ALTER TABLE BIOSYS.USRIN$LOG
   DROP CONSTRAINT ADDR$LOG_FK;

ALTER TABLE BIOSYS.USRLOCK
   DROP CONSTRAINT USR$LCK_FK;

ALTER TABLE BIOSYS.USRLOG
   DROP CONSTRAINT USR$LOG_FK;

ALTER TABLE BIOSYS.USRRLE
   DROP CONSTRAINT ROLE$USR_FK;

ALTER TABLE BIOSYS.USRRLE
   DROP CONSTRAINT USR$ROLE_FK;

ALTER TABLE BIOSYS.UWORKSPACE
   DROP CONSTRAINT USR$WS_FK;

DROP INDEX BIOSYS.WS$ORG_FK;

DROP INDEX BIOSYS.ORG$ORG_FK;

DROP TABLE BIOSYS.ORG CASCADE CONSTRAINTS;

DROP TABLE BIOSYS.RADDRSS CASCADE CONSTRAINTS;

DROP TABLE BIOSYS.SMTP$LOG CASCADE CONSTRAINTS;

DROP TABLE BIOSYS.UGRANT CASCADE CONSTRAINTS;

DROP TABLE BIOSYS.UROLE CASCADE CONSTRAINTS;

DROP INDEX BIOSYS.WS$USR_FK;

DROP INDEX BIOSYS.ORG$USR_FK;

DROP INDEX BIOSYS.IX_USRS_GBRG;

DROP TABLE BIOSYS.USR CASCADE CONSTRAINTS;

DROP INDEX BIOSYS.GRANT$USR_FK;

DROP INDEX BIOSYS.USR$GRANT_FK;

DROP TABLE BIOSYS.USRGRNT CASCADE CONSTRAINTS;

DROP INDEX BIOSYS.ADDR$LOG_FK;

DROP TABLE BIOSYS.USRIN$LOG CASCADE CONSTRAINTS;

DROP INDEX BIOSYS.USR$LCK_FK;

DROP TABLE BIOSYS.USRLOCK CASCADE CONSTRAINTS;

DROP INDEX BIOSYS.USR$LOG_FK;

DROP TABLE BIOSYS.USRLOG CASCADE CONSTRAINTS;

DROP INDEX BIOSYS.USR$ROLE_FK;

DROP INDEX BIOSYS.ROLE$USR_FK;

DROP TABLE BIOSYS.USRRLE CASCADE CONSTRAINTS;

DROP INDEX BIOSYS.USR$WS_FK;

DROP TABLE BIOSYS.UWORKSPACE CASCADE CONSTRAINTS;

DROP USER BIOSYS;

/*==============================================================*/
/* User: BIOSYS                                                 */
/*==============================================================*/
CREATE USER BIOSYS 
  identified by "";

/*==============================================================*/
/* Table: ORG                                                   */
/*==============================================================*/
CREATE TABLE BIOSYS.ORG  (
   ORG_ID               NUMBER(18)                      NOT NULL,
   PRNT_ORG_ID          NUMBER(18),
   WORKSPACE_ID         NUMBER(18)                      NOT NULL,
   ANAME                VARCHAR2(500)                   NOT NULL,
   ADESC                VARCHAR2(1000),
   CONSTRAINT PK_ORG PRIMARY KEY (ORG_ID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.ORG IS
'Подразделение';

COMMENT ON COLUMN BIOSYS.ORG.ORG_ID IS
'ID подразделения';

COMMENT ON COLUMN BIOSYS.ORG.PRNT_ORG_ID IS
'ID подразделения-родителя';

COMMENT ON COLUMN BIOSYS.ORG.WORKSPACE_ID IS
'ID пространства';

COMMENT ON COLUMN BIOSYS.ORG.ANAME IS
'Название подразделения';

COMMENT ON COLUMN BIOSYS.ORG.ADESC IS
'Описание подразделения';

/*==============================================================*/
/* Index: ORG$ORG_FK                                            */
/*==============================================================*/
CREATE INDEX BIOSYS.ORG$ORG_FK ON BIOSYS.ORG (
   PRNT_ORG_ID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Index: WS$ORG_FK                                             */
/*==============================================================*/
CREATE INDEX BIOSYS.WS$ORG_FK ON BIOSYS.ORG (
   WORKSPACE_ID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Table: RADDRSS                                               */
/*==============================================================*/
CREATE TABLE BIOSYS.RADDRSS  (
   REM_ADDR             VARCHAR2(32)                    NOT NULL,
   REM_HOST             VARCHAR2(100)                   NOT NULL,
   ADDR_DESC            VARCHAR2(1000),
   CONSTRAINT PK_RADDRSS PRIMARY KEY (REM_ADDR)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.RADDRSS IS
'Справочник адресов';

COMMENT ON COLUMN BIOSYS.RADDRSS.REM_ADDR IS
'Адрес пользователя';

COMMENT ON COLUMN BIOSYS.RADDRSS.REM_HOST IS
'Имя хоста пользователя';

COMMENT ON COLUMN BIOSYS.RADDRSS.ADDR_DESC IS
'Описание';

/*==============================================================*/
/* Table: SMTP$LOG                                              */
/*==============================================================*/
CREATE TABLE BIOSYS.SMTP$LOG  (
   ERR_UID              VARCHAR2(32)                    NOT NULL
      CONSTRAINT CKC_ERR_UID_SMTP$LOG CHECK (ERR_UID = UPPER(ERR_UID)),
   CRE_DATE             DATE                            NOT NULL,
   SRV_ADDR             VARCHAR2(100)                   NOT NULL,
   PORT_NUM             NUMBER(6)                       NOT NULL,
   USR_LOGIN            VARCHAR2(64),
   USR_PWD              VARCHAR2(32),
   ADDR_FROM            VARCHAR2(100)                   NOT NULL,
   ADDR_TO              VARCHAR2(100)                   NOT NULL,
   MSG_SUBJ             VARCHAR2(1000),
   ERROR_TEXT           CLOB                            NOT NULL,
   CONSTRAINT PK_SMTP$LOG PRIMARY KEY (ERR_UID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.SMTP$LOG IS
'Лог ошибок отправки сообщений по SMTP';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.ERR_UID IS
'UID ошибки';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.CRE_DATE IS
'Дата/время';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.SRV_ADDR IS
'Сервер';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.PORT_NUM IS
'Порт';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.USR_LOGIN IS
'Пользователь';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.USR_PWD IS
'Пароль';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.ADDR_FROM IS
'Адрес отправителя';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.ADDR_TO IS
'Адрес получателя';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.MSG_SUBJ IS
'Тема сообщения';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.ERROR_TEXT IS
'Текст ошибки';

/*==============================================================*/
/* Table: UGRANT                                                */
/*==============================================================*/
CREATE TABLE BIOSYS.UGRANT  (
   GRANT_UID            VARCHAR2(64)                    NOT NULL
      CONSTRAINT CKC_GRANT_UID_UGRANT CHECK (GRANT_UID = UPPER(GRANT_UID)),
   ANAME                VARCHAR2(200)                   NOT NULL,
   IS_SYS               CHAR(1)                        DEFAULT '0' NOT NULL
      CONSTRAINT CKC_IS_SYS_UGRANT CHECK (IS_SYS IN ('0','1')),
   ADESC                VARCHAR2(1000),
   CONSTRAINT PK_UGRANT PRIMARY KEY (GRANT_UID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.UGRANT IS
'Разрешение';

COMMENT ON COLUMN BIOSYS.UGRANT.GRANT_UID IS
'UID разрешения';

COMMENT ON COLUMN BIOSYS.UGRANT.ANAME IS
'Имя разрешения';

COMMENT ON COLUMN BIOSYS.UGRANT.IS_SYS IS
'Системное разрешения (0-False; 1-True)';

COMMENT ON COLUMN BIOSYS.UGRANT.ADESC IS
'Описание роли';

/*==============================================================*/
/* Table: UROLE                                                 */
/*==============================================================*/
CREATE TABLE BIOSYS.UROLE  (
   ROLE_UID             VARCHAR2(64)                    NOT NULL
      CONSTRAINT CKC_ROLE_UID_UROLE CHECK (ROLE_UID = UPPER(ROLE_UID)),
   ANAME                VARCHAR2(200)                   NOT NULL,
   IS_SYS               CHAR(1)                        DEFAULT '0' NOT NULL
      CONSTRAINT CKC_IS_SYS_UROLE CHECK (IS_SYS IN ('0','1')),
   ADESC                VARCHAR2(1000),
   CONSTRAINT PK_UROLE PRIMARY KEY (ROLE_UID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.UROLE IS
'Роль';

COMMENT ON COLUMN BIOSYS.UROLE.ROLE_UID IS
'UID роли';

COMMENT ON COLUMN BIOSYS.UROLE.ANAME IS
'Имя роли';

COMMENT ON COLUMN BIOSYS.UROLE.IS_SYS IS
'Системная роль (0-False; 1-True)';

COMMENT ON COLUMN BIOSYS.UROLE.ADESC IS
'Описание роли';

/*==============================================================*/
/* Table: USR                                                   */
/*==============================================================*/
CREATE TABLE BIOSYS.USR  (
   USR_UID              VARCHAR2(32)                    NOT NULL
      CONSTRAINT CKC_USR_UID_USR CHECK (USR_UID = UPPER(USR_UID)),
   ORG_ID               NUMBER(18),
   WORKSPACE_ID         NUMBER(18)                      NOT NULL,
   USR_LOGIN            VARCHAR2(64)                    NOT NULL
      CONSTRAINT CKC_USR_LOGIN_USR CHECK (USR_LOGIN = LOWER(USR_LOGIN)),
   USR_PWD              VARCHAR2(32)                    NOT NULL,
   FIO_FAM              VARCHAR2(100)                   NOT NULL
      CONSTRAINT CKC_FIO_FAM_USR CHECK (FIO_FAM = UPPER(FIO_FAM)),
   FIO_FNAME            VARCHAR2(100)                   NOT NULL
      CONSTRAINT CKC_FIO_FNAME_USR CHECK (FIO_FNAME = UPPER(FIO_FNAME)),
   FIO_SNAME            VARCHAR2(100)                   NOT NULL
      CONSTRAINT CKC_FIO_SNAME_USR CHECK (FIO_SNAME = UPPER(FIO_SNAME)),
   REG_DATE             DATE                            NOT NULL,
   EMAIL_ADDR           VARCHAR2(100)                   NOT NULL,
   USR_PHONE            VARCHAR2(12),
   CONFIRMED            CHAR(1)                        DEFAULT '0'
      CONSTRAINT CKC_CONFIRMED_USR CHECK (CONFIRMED IS NULL OR (CONFIRMED IN ('0','1'))),
   GARBAGED             CHAR(1)                        DEFAULT '0' NOT NULL
      CONSTRAINT CKC_GARBAGED_USR CHECK (GARBAGED IN ('0','1')),
   EXTINFO              VARCHAR2(1000),
   CONSTRAINT PK_USR PRIMARY KEY (USR_UID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.USR IS
'Пользователи';

COMMENT ON COLUMN BIOSYS.USR.USR_UID IS
'UID пользователя';

COMMENT ON COLUMN BIOSYS.USR.ORG_ID IS
'ID подразделения';

COMMENT ON COLUMN BIOSYS.USR.WORKSPACE_ID IS
'ID пространства';

COMMENT ON COLUMN BIOSYS.USR.USR_LOGIN IS
'Логин';

COMMENT ON COLUMN BIOSYS.USR.USR_PWD IS
'Пароль';

COMMENT ON COLUMN BIOSYS.USR.FIO_FAM IS
'Фамилия';

COMMENT ON COLUMN BIOSYS.USR.FIO_FNAME IS
'Имя';

COMMENT ON COLUMN BIOSYS.USR.FIO_SNAME IS
'Отчество';

COMMENT ON COLUMN BIOSYS.USR.REG_DATE IS
'Дата регистрации';

COMMENT ON COLUMN BIOSYS.USR.EMAIL_ADDR IS
'e-mail';

COMMENT ON COLUMN BIOSYS.USR.USR_PHONE IS
'Телефон';

COMMENT ON COLUMN BIOSYS.USR.CONFIRMED IS
'Подтверждение (0-False; 1-True)';

COMMENT ON COLUMN BIOSYS.USR.GARBAGED IS
'В мусор (0-False; 1-True)';

COMMENT ON COLUMN BIOSYS.USR.EXTINFO IS
'Дополнительная информация';

/*==============================================================*/
/* Index: IX_USRS_GBRG                                          */
/*==============================================================*/
CREATE INDEX BIOSYS.IX_USRS_GBRG ON BIOSYS.USR (
   GARBAGED ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Index: ORG$USR_FK                                            */
/*==============================================================*/
CREATE INDEX BIOSYS.ORG$USR_FK ON BIOSYS.USR (
   ORG_ID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Index: WS$USR_FK                                             */
/*==============================================================*/
CREATE INDEX BIOSYS.WS$USR_FK ON BIOSYS.USR (
   WORKSPACE_ID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Table: USRGRNT                                               */
/*==============================================================*/
CREATE TABLE BIOSYS.USRGRNT  (
   GRANT_UID            VARCHAR2(64)                    NOT NULL
      CONSTRAINT CKC_GRANT_UID_USRGRNT CHECK (GRANT_UID = UPPER(GRANT_UID)),
   USR_UID              VARCHAR2(32)                    NOT NULL
      CONSTRAINT CKC_USR_UID_USRGRNT CHECK (USR_UID = UPPER(USR_UID)),
   CONSTRAINT PK_USRGRNT PRIMARY KEY (GRANT_UID, USR_UID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.USRGRNT IS
'Разрешение пользователя';

COMMENT ON COLUMN BIOSYS.USRGRNT.GRANT_UID IS
'UID разрешения';

COMMENT ON COLUMN BIOSYS.USRGRNT.USR_UID IS
'UID пользователя';

/*==============================================================*/
/* Index: USR$GRANT_FK                                          */
/*==============================================================*/
CREATE INDEX BIOSYS.USR$GRANT_FK ON BIOSYS.USRGRNT (
   USR_UID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Index: GRANT$USR_FK                                          */
/*==============================================================*/
CREATE INDEX BIOSYS.GRANT$USR_FK ON BIOSYS.USRGRNT (
   GRANT_UID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Table: USRIN$LOG                                             */
/*==============================================================*/
CREATE TABLE BIOSYS.USRIN$LOG  (
   REC_ID               NUMBER(18)                      NOT NULL,
   REM_ADDR             VARCHAR2(32)                    NOT NULL,
   USR_LOGIN            VARCHAR2(64)                    NOT NULL,
   SESSION_ID           VARCHAR2(32)                    NOT NULL
      CONSTRAINT CKC_SESSION_ID_USRIN$LO CHECK (SESSION_ID = UPPER(SESSION_ID)),
   REM_CLIENT           VARCHAR2(1000)                  NOT NULL,
   ASTATUS              VARCHAR2(200)                   NOT NULL,
   USRIN_DATE           DATE                            NOT NULL,
   CONSTRAINT PK_USRIN$LOG PRIMARY KEY (REC_ID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.USRIN$LOG IS
'Лог входов в систему';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.REC_ID IS
'ID записи';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.REM_ADDR IS
'Адрес пользователя';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.USR_LOGIN IS
'Логин';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.SESSION_ID IS
'ID Сессии';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.REM_CLIENT IS
'Клиент пользователя';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.ASTATUS IS
'Статус';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.USRIN_DATE IS
'Дата входа';

/*==============================================================*/
/* Index: ADDR$LOG_FK                                           */
/*==============================================================*/
CREATE INDEX BIOSYS.ADDR$LOG_FK ON BIOSYS.USRIN$LOG (
   REM_ADDR ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Table: USRLOCK                                               */
/*==============================================================*/
CREATE TABLE BIOSYS.USRLOCK  (
   LOCK_ID              NUMBER(18)                      NOT NULL,
   USR_UID              VARCHAR2(32)                    NOT NULL
      CONSTRAINT CKC_USR_UID_USRLOCK CHECK (USR_UID = UPPER(USR_UID)),
   LOCK_TYPE            CHAR(1)                        DEFAULT '0' NOT NULL
      CONSTRAINT CKC_LOCK_TYPE_USRLOCK CHECK (LOCK_TYPE IN ('0','1')),
   CREATED              DATE                            NOT NULL,
   FROM_POINT           DATE                            NOT NULL,
   TO_POINT             DATE,
   COMMENTS             VARCHAR2(4000),
   DELETED              CHAR(1)                        DEFAULT '0' NOT NULL
      CONSTRAINT CKC_DELETED_USRLOCK CHECK (DELETED IN ('0','1')),
   CONSTRAINT PK_USRLOCK PRIMARY KEY (LOCK_ID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.USRLOCK IS
'Блокировка пользователя. Данные пользователи блокируются на указанный период';

COMMENT ON COLUMN BIOSYS.USRLOCK.LOCK_ID IS
'ID блокировки';

COMMENT ON COLUMN BIOSYS.USRLOCK.USR_UID IS
'UID пользователя';

COMMENT ON COLUMN BIOSYS.USRLOCK.LOCK_TYPE IS
'Тип блокировки (0-ручная; 1-автоматическая)';

COMMENT ON COLUMN BIOSYS.USRLOCK.CREATED IS
'Дата/Время создания';

COMMENT ON COLUMN BIOSYS.USRLOCK.FROM_POINT IS
'Дата/Время начала блокировки';

COMMENT ON COLUMN BIOSYS.USRLOCK.TO_POINT IS
'Дата/Время окончания блокировки';

COMMENT ON COLUMN BIOSYS.USRLOCK.COMMENTS IS
'Комментарии';

COMMENT ON COLUMN BIOSYS.USRLOCK.DELETED IS
'Блокировка удалена (0-False; 1-True)';

/*==============================================================*/
/* Index: USR$LCK_FK                                            */
/*==============================================================*/
CREATE INDEX BIOSYS.USR$LCK_FK ON BIOSYS.USRLOCK (
   USR_UID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Table: USRLOG                                                */
/*==============================================================*/
CREATE TABLE BIOSYS.USRLOG  (
   USRLOG_ID            NUMBER(18)                      NOT NULL,
   USR_UID              VARCHAR2(32)                    NOT NULL
      CONSTRAINT CKC_USR_UID_USRLOG CHECK (USR_UID = UPPER(USR_UID)),
   IOBJ_CD              VARCHAR2(30)                    NOT NULL,
   IOBJ_ID              VARCHAR2(32)                    NOT NULL
      CONSTRAINT CKC_IOBJ_ID_USRLOG CHECK (IOBJ_ID = UPPER(IOBJ_ID)),
   ACTION_TEXT          CLOB                            NOT NULL,
   ACT_DATE             DATE                            NOT NULL,
   IOBJ_MASTER_CD       VARCHAR2(30),
   IOBJ_MASTER_ID       VARCHAR2(32)                   
      CONSTRAINT CKC_IOBJ_MASTER_ID_USRLOG CHECK (IOBJ_MASTER_ID IS NULL OR (IOBJ_MASTER_ID = UPPER(IOBJ_MASTER_ID))),
   CONSTRAINT PK_USRLOG PRIMARY KEY (USRLOG_ID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.USRLOG IS
'Протокол действий пользователя';

COMMENT ON COLUMN BIOSYS.USRLOG.USRLOG_ID IS
'ID записи';

COMMENT ON COLUMN BIOSYS.USRLOG.USR_UID IS
'UID пользователя';

COMMENT ON COLUMN BIOSYS.USRLOG.IOBJ_CD IS
'Код измененного объекта (Имя таблицы)';

COMMENT ON COLUMN BIOSYS.USRLOG.IOBJ_ID IS
'ID объекта';

COMMENT ON COLUMN BIOSYS.USRLOG.ACTION_TEXT IS
'Описание действия';

COMMENT ON COLUMN BIOSYS.USRLOG.ACT_DATE IS
'Дата и время действия';

COMMENT ON COLUMN BIOSYS.USRLOG.IOBJ_MASTER_CD IS
'Код измененного master-объекта (Имя таблицы)';

COMMENT ON COLUMN BIOSYS.USRLOG.IOBJ_MASTER_ID IS
'ID master-объекта';

/*==============================================================*/
/* Index: USR$LOG_FK                                            */
/*==============================================================*/
CREATE INDEX BIOSYS.USR$LOG_FK ON BIOSYS.USRLOG (
   USR_UID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Table: USRRLE                                                */
/*==============================================================*/
CREATE TABLE BIOSYS.USRRLE  (
   ROLE_UID             VARCHAR2(64)                    NOT NULL
      CONSTRAINT CKC_ROLE_UID_USRRLE CHECK (ROLE_UID = UPPER(ROLE_UID)),
   USR_UID              VARCHAR2(32)                    NOT NULL
      CONSTRAINT CKC_USR_UID_USRRLE CHECK (USR_UID = UPPER(USR_UID)),
   CONSTRAINT PK_USRRLE PRIMARY KEY (ROLE_UID, USR_UID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.USRRLE IS
'Роль пользователя';

COMMENT ON COLUMN BIOSYS.USRRLE.ROLE_UID IS
'UID роли';

COMMENT ON COLUMN BIOSYS.USRRLE.USR_UID IS
'UID пользователя';

/*==============================================================*/
/* Index: ROLE$USR_FK                                           */
/*==============================================================*/
CREATE INDEX BIOSYS.ROLE$USR_FK ON BIOSYS.USRRLE (
   ROLE_UID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Index: USR$ROLE_FK                                           */
/*==============================================================*/
CREATE INDEX BIOSYS.USR$ROLE_FK ON BIOSYS.USRRLE (
   USR_UID ASC
)
TABLESPACE BIOSYS_INDX;

/*==============================================================*/
/* Table: UWORKSPACE                                            */
/*==============================================================*/
CREATE TABLE BIOSYS.UWORKSPACE  (
   WORKSPACE_ID         NUMBER(18)                      NOT NULL,
   USR_UID              VARCHAR2(32)                   
      CONSTRAINT CKC_USR_UID_UWORKSPA CHECK (USR_UID IS NULL OR (USR_UID = UPPER(USR_UID))),
   ANAME                VARCHAR2(200)                   NOT NULL,
   ADESC                VARCHAR2(1000),
   CONSTRAINT PK_UWORKSPACE PRIMARY KEY (WORKSPACE_ID)
         USING INDEX TABLESPACE BIOSYS_INDX
);

COMMENT ON TABLE BIOSYS.UWORKSPACE IS
'Пространство';

COMMENT ON COLUMN BIOSYS.UWORKSPACE.WORKSPACE_ID IS
'ID пространства';

COMMENT ON COLUMN BIOSYS.UWORKSPACE.USR_UID IS
'UID пользователя';

COMMENT ON COLUMN BIOSYS.UWORKSPACE.ANAME IS
'Имя пространства';

COMMENT ON COLUMN BIOSYS.UWORKSPACE.ADESC IS
'Описание пространства';

/*==============================================================*/
/* Index: USR$WS_FK                                             */
/*==============================================================*/
CREATE INDEX BIOSYS.USR$WS_FK ON BIOSYS.UWORKSPACE (
   USR_UID ASC
)
TABLESPACE BIOSYS_INDX;

ALTER TABLE BIOSYS.ORG
   ADD CONSTRAINT ORG$ORG_FK FOREIGN KEY (PRNT_ORG_ID)
      REFERENCES BIOSYS.ORG (ORG_ID);

ALTER TABLE BIOSYS.ORG
   ADD CONSTRAINT WS$ORG_FK FOREIGN KEY (WORKSPACE_ID)
      REFERENCES BIOSYS.UWORKSPACE (WORKSPACE_ID);

ALTER TABLE BIOSYS.USR
   ADD CONSTRAINT ORG$USR_FK FOREIGN KEY (ORG_ID)
      REFERENCES BIOSYS.ORG (ORG_ID);

ALTER TABLE BIOSYS.USR
   ADD CONSTRAINT WS$USR_FK FOREIGN KEY (WORKSPACE_ID)
      REFERENCES BIOSYS.UWORKSPACE (WORKSPACE_ID);

ALTER TABLE BIOSYS.USRGRNT
   ADD CONSTRAINT GRANT$USR_FK FOREIGN KEY (GRANT_UID)
      REFERENCES BIOSYS.UGRANT (GRANT_UID);

ALTER TABLE BIOSYS.USRGRNT
   ADD CONSTRAINT USR$GRANT_FK FOREIGN KEY (USR_UID)
      REFERENCES BIOSYS.USR (USR_UID);

ALTER TABLE BIOSYS.USRIN$LOG
   ADD CONSTRAINT ADDR$LOG_FK FOREIGN KEY (REM_ADDR)
      REFERENCES BIOSYS.RADDRSS (REM_ADDR);

ALTER TABLE BIOSYS.USRLOCK
   ADD CONSTRAINT USR$LCK_FK FOREIGN KEY (USR_UID)
      REFERENCES BIOSYS.USR (USR_UID);

ALTER TABLE BIOSYS.USRLOG
   ADD CONSTRAINT USR$LOG_FK FOREIGN KEY (USR_UID)
      REFERENCES BIOSYS.USR (USR_UID);

ALTER TABLE BIOSYS.USRRLE
   ADD CONSTRAINT ROLE$USR_FK FOREIGN KEY (ROLE_UID)
      REFERENCES BIOSYS.UROLE (ROLE_UID);

ALTER TABLE BIOSYS.USRRLE
   ADD CONSTRAINT USR$ROLE_FK FOREIGN KEY (USR_UID)
      REFERENCES BIOSYS.USR (USR_UID);

ALTER TABLE BIOSYS.UWORKSPACE
   ADD CONSTRAINT USR$WS_FK FOREIGN KEY (USR_UID)
      REFERENCES BIOSYS.USR (USR_UID);

