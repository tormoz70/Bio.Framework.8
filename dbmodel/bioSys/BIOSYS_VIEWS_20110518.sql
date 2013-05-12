-- Start of DDL Script for View BIOSYS.AI_DBS
-- Generated 18-май-2011 22:28:27 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE VIEW ai_dbs (
   con_uid,
   con_str,
   is_default )
AS
SELECT a.con_uid, a.con_str, a.is_default
  FROM dbcons a
/


-- End of DDL Script for View BIOSYS.AI_DBS

-- Start of DDL Script for View BIOSYS.AI_ORGSTRUCT
-- Generated 18-май-2011 22:28:28 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE VIEW ai_orgstruct (
   odep_uid,
   odep_prnt,
   odep_prnt_conn,
   odep_name,
   odep_desc,
   odep_uid_auto,
   owner_uid )
AS
SELECT 
  a.odep_uid, 
  nvl(a.odep_prnt, 'ROOT-NODE') as odep_prnt,
  a.odep_prnt as odep_prnt_conn,
  a.odep_name,
  a.odep_desc,
  a.odep_uid_auto,
  ai_admin.get_odep_owner(a.odep_uid) as owner_uid
  FROM orgstruct a
/


-- End of DDL Script for View BIOSYS.AI_ORGSTRUCT

-- Start of DDL Script for View BIOSYS.AI_ORGSTRUCT_PATH
-- Generated 18-май-2011 22:28:29 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE VIEW ai_orgstruct_path (
   odep_uid,
   odep_prnt,
   odep_name,
   odep_desc,
   f_path,
   f_uid_path )
AS
select ODEP_UID, ODEP_PRNT, ODEP_NAME, ODEP_DESC, replace(F_PATH, '|-|', '/') as F_PATH, replace(F_UID_PATH, '|-|', '/') as F_UID_PATH from(
select a.odep_uid, nvl(a.odep_prnt, 'ROOT-NODE') as odep_prnt, a.odep_name, a.odep_desc,
      sys_connect_by_path(a.odep_name, '|-|') as f_path,
      sys_connect_by_path(a.odep_uid, '|-|') as f_uid_path
      from orgstruct a
     start with a.odep_prnt is null
    connect by prior a.odep_uid = a.odep_prnt)
/

-- Grants for View
GRANT SELECT ON ai_orgstruct_path TO public
/

-- End of DDL Script for View BIOSYS.AI_ORGSTRUCT_PATH

-- Start of DDL Script for View BIOSYS.AI_RADDRSS
-- Generated 18-май-2011 22:28:29 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE VIEW ai_raddrss (
   rem_addr,
   rem_host,
   addr_desc,
   lastin_date )
AS
SELECT '*' as rem_addr, 'все адреса' as rem_host, '' as addr_desc, null as lastin_date
  FROM dual
UNION
SELECT a.rem_addr, a.rem_host, a.addr_desc,
       (select max(b.usrin_date) from AI_USRIN_LOG b where b.rem_addr = a.rem_addr) as lastin_date
  FROM raddrss a
/

-- Grants for View
GRANT SELECT ON ai_raddrss TO public
/

-- End of DDL Script for View BIOSYS.AI_RADDRSS

-- Start of DDL Script for View BIOSYS.AI_UROLES
-- Generated 18-май-2011 22:28:30 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE VIEW ai_uroles (
   role_uid,
   role_name,
   role_is_sys,
   role_desc )
AS
SELECT a.role_uid, a.role_name, a.role_is_sys, a.role_desc
  FROM uroles a
/


-- End of DDL Script for View BIOSYS.AI_UROLES

-- Start of DDL Script for View BIOSYS.AI_USRIN_LOG
-- Generated 18-май-2011 22:28:30 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE VIEW ai_usrin_log (
   rem_addr,
   usrin_num,
   usr_name,
   session_id,
   rem_client,
   astatus,
   usrin_date )
AS
SELECT a.rem_addr, a.usrin_num, a.usr_name, a.session_id,
       a.rem_client, a.astatus, a.usrin_date
  FROM usrin$log a
 ORDER BY a.usrin_date DESC
/

-- Grants for View
GRANT SELECT ON ai_usrin_log TO public
/

-- End of DDL Script for View BIOSYS.AI_USRIN_LOG

-- Start of DDL Script for View BIOSYS.AI_USRS
-- Generated 18-май-2011 22:28:31 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE VIEW ai_usrs (
   usr_uid,
   odep_uid,
   odep_path,
   con_uid,
   usr_name,
   usr_pwd,
   usr_fam,
   usr_fname,
   usr_sname,
   usr_fio,
   usr_fio_shrt,
   reg_date,
   blocked,
   email_addr,
   usr_phone,
   confirmed,
   garbaged,
   extinfo )
AS
SELECT a.usr_uid, 
       a.odep_uid, /*(select max(o.f_path) from ai_orgstruct_path o where a.odep_uid = o.odep_uid)*/ o.f_path as odep_path,
       a.con_uid, a.usr_name, a.usr_pwd,
       initcap(trim(a.usr_fam)) as usr_fam, initcap(trim(a.usr_fname)) as usr_fname, initcap(trim(a.usr_sname)) as usr_sname,
       initcap(trim(a.usr_fam))|| ' ' ||initcap(trim(a.usr_fname))|| ' ' ||initcap(trim(a.usr_sname)) as usr_fio,
       initcap(trim(a.usr_fam))|| ' ' ||substr(trim(a.usr_fname), 1, 1)|| '. ' ||substr(upper(a.usr_sname), 1, 1)||'.' as usr_fio_shrt,
       a.reg_date, a.blocked, a.email_addr, a.usr_phone,
       a.confirmed, a.garbaged, a.extinfo
  FROM usrs a, ai_orgstruct_path o
 WHERE a.odep_uid = o.odep_uid
/


-- End of DDL Script for View BIOSYS.AI_USRS

-- Start of DDL Script for View BIOSYS.AI_USRS_EDLIST
-- Generated 18-май-2011 22:28:31 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE VIEW ai_usrs_edlist (
   usr_uid,
   con_uid,
   con_str,
   usr_name,
   usr_pwd,
   usr_fam,
   usr_fname,
   usr_sname,
   reg_date,
   blocked,
   blocked_cptn,
   usr_roles,
   email_addr,
   usr_phone,
   odep_uid,
   odep_path,
   odep_uid_path,
   odep_name,
   odep_desc,
   confirmed,
   confirmed_cptn,
   owner_uid,
   garbaged,
   is_admin,
   is_wsadmin,
   extinfo )
AS
SELECT 
  a.usr_uid, 
  a.con_uid, 
  d.con_str,
  a.usr_name, 
  a.usr_pwd,
  a.usr_fam,
  a.usr_fname,
  a.usr_sname,
  a.reg_date,
  a.blocked, decode(a.blocked, 1, 'да', '') as blocked_cptn,
  null as usr_roles,
  a.email_addr,
  a.usr_phone,
  a.odep_uid,
  ltrim(o.f_path, '/') as odep_path,
  ltrim(o.f_uid_path, '/') as odep_uid_path,
  o.odep_name,
  o.odep_desc,
  a.confirmed, decode(a.confirmed, 1, 'активирован', '') as confirmed_cptn,
  null as owner_uid,
  a.garbaged,
  0 as is_admin,
  0 as is_wsadmin,
  a.extinfo
  
  FROM ai_usrs a, ai_orgstruct_path o, dbcons d
 WHERE a.odep_uid = o.odep_uid
   AND d.con_uid = a.con_uid
/


-- End of DDL Script for View BIOSYS.AI_USRS_EDLIST

