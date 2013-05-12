CREATE TABLESPACE BIOSYS_DATA DATAFILE 'c:\oraclexe\app\oracle\oradata\XE\BIOSYS_DATA.ORA' SIZE 100M;
CREATE TABLESPACE BIOSYS_INDX DATAFILE 'c:\oraclexe\app\oracle\oradata\XE\BIOSYS_INDX.ORA' SIZE 100M;

drop user biosys cascade
/

CREATE USER biosys
IDENTIFIED BY j12
DEFAULT TABLESPACE BIOSYS_DATA
TEMPORARY TABLESPACE TEMP
/
GRANT ALTER ANY PROCEDURE TO biosys
/
GRANT CREATE ANY DIRECTORY TO biosys
/
GRANT CREATE ANY SEQUENCE TO biosys
/
GRANT CREATE ANY TABLE TO biosys
/
GRANT CREATE ANY VIEW TO biosys
/
GRANT CREATE PROCEDURE TO biosys
/
GRANT CREATE SESSION TO biosys
/
GRANT DROP ANY DIRECTORY TO biosys
/
GRANT DROP ANY SEQUENCE TO biosys
/
GRANT SELECT ANY TABLE TO biosys
/
GRANT UNLIMITED TABLESPACE TO biosys
/
GRANT AQ_ADMINISTRATOR_ROLE TO biosys
/
GRANT DBA TO biosys
/
GRANT RESOURCE TO biosys
/
ALTER USER biosys DEFAULT ROLE ALL
/
GRANT SELECT ON dba_datapump_jobs TO biosys
/
GRANT SELECT ON dba_tab_columns TO biosys
/
GRANT EXECUTE ON dbms_lock TO biosys
/
GRANT EXECUTE ON dbms_lock TO biosys
/
GRANT EXECUTE ON dbms_session TO biosys
/
GRANT EXECUTE ON dbms_session TO biosys
/
GRANT EXECUTE ON utl_smtp TO biosys
/

exec DBMS_NETWORK_ACL_ADMIN.DROP_ACL(acl => 'send_mail.xml');
exec DBMS_NETWORK_ACL_ADMIN.CREATE_ACL(acl => 'send_mail.xml',description => 'send_mail ACL',principal => 'BIOSYS',is_grant => true,privilege => 'connect');
exec DBMS_NETWORK_ACL_ADMIN.ADD_PRIVILEGE(acl => 'send_mail.xml',principal => 'BIOSYS',is_grant  => true,privilege => 'resolve');
exec DBMS_NETWORK_ACL_ADMIN.ASSIGN_ACL(acl => 'send_mail.xml',host => 'mail.givc.ru'); -- this ip is out smtp server ip
commit;

select COMP_NAME, status from dba_registry;
