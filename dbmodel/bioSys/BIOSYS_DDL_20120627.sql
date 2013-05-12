DROP PACKAGE BIOSYS.AI_ADMIN;

CREATE OR REPLACE PACKAGE BIOSYS.AI_ADMIN
  IS

  cs_dont_change_value_flag varchar2(32) := '$dont_change_value_of_field';
  csADMIN_ROLE_UID varchar2(32) := 'BIO-ADMIN';
  csWSADMIN_ROLE_UID varchar2(32) := 'BIO-WSADMIN';

  type t_usr_rec is record(
    usr_uid                     usrs.usr_uid%type,
    con_uid                     dbcons.con_uid%type,
    con_str                     dbcons.con_str%type,
    usr_name                    usrs.usr_name%type,
    usr_pwd                     usrs.usr_pwd%type,
    usr_fam                     usrs.usr_fam%type,
    usr_fname                   usrs.usr_fname%type,
    usr_sname                   usrs.usr_sname%type,
    reg_date                    usrs.reg_date%type,
    blocked                     usrs.blocked%type,
    blocked_cptn                VARCHAR2(100),
    usr_roles                   VARCHAR2(4000),
    email_addr                  usrs.email_addr%type,
    usr_phone                   usrs.usr_phone%type,
    odep_uid                    orgstruct.odep_uid%type,
    odep_path                   VARCHAR2(4000),
    odep_uid_path               VARCHAR2(4000),
    odep_name                   orgstruct.odep_name%type,
    odep_desc                   orgstruct.odep_desc%type,
    confirmed                   usrs.confirmed%type,
    confirmed_cptn              VARCHAR2(100),
    owner_uid                   usrs.usr_uid%type,
    garbaged                    usrs.garbaged%type,
    is_admin                    number(1),
    is_wsadmin                  number(1),
    extinfo                     usrs.extinfo%type
        );
  type t_usr_tbl is table of t_usr_rec;

  type t_usrole_rec is record(
     role_uid      uroles.role_uid%type,
     role_name      uroles.role_name%type,
     role_is_sys       uroles.role_is_sys%type,
     role_desc      uroles.role_desc%type,
     usr_uid   usrs.usr_uid%type
  );
  type t_usrole_tbl is table of t_usrole_rec;

  --type t_usrlst_tbl is table of ai_usrs_edlist%rowtype;

  type t_node_det_rec is record(
    f_uid orgstruct.odep_uid%type,
    f_text orgstruct.odep_name%type,
    f_extinfo usrs.extinfo%type,
    f_icon varchar2(4)
    /*
      флаги в поле fa_icon:
        поз 1: 0-сотрудник; 1-отдел
        поз 2: 0-неактивирован; 1-активирован
        поз 3: 0-незаблокирован; 1-заблокирован
        поз 4: 0-неудален; 1-удален в корзину
    */
  );
  type t_node_det_tbl is table of t_node_det_rec;

  type T_NODE_REC is record(
    f_uid orgstruct.odep_uid%type,
    f_parent_uid orgstruct.odep_uid%type,
    f_text orgstruct.odep_name%type,
    f_cls varchar2(100),
    f_data varchar2(4000),
    f_count number(10),
    f_icon varchar2(500)
  );
  type T_NODE_TBL is table of T_NODE_REC;

  procedure get_reg_card(
    p_act in varchar2,
    p_forceactivate in varchar2,
    p_biosystitle in varchar2,
    p_useruid in varchar2,
    p_cnfrmurl in varchar2,
    v_sendto out varchar2, v_subject out varchar2, v_body out varchar2);
  function usr_roles(
    pUsrUID in usrs.usr_uid%type,
    pInvert in number default 0
    ) return t_usrole_tbl pipelined;
  function get_usr_roles(p_usr_uid in usrs.usr_uid%type) return varchar2;
  function get_usr_caption(
    p_usr_name in varchar2,
    p_usr_fam in varchar2,
    p_usr_fname in varchar2,
    p_usr_sname in varchar2
    ) return varchar2;
  function get_org_node_det(
    p_node_uid in orgstruct.odep_uid%type,
    p_cur_usr in usrs.usr_uid%type) return T_NODE_DET_TBL pipelined;
  function usr_has_role(
    pUsrUID in usrs.usr_uid%type,
    pRole in uroles.role_uid%type) return number;
  procedure set_usr_roles(
    p_usr_uid in varchar2,
    p_roles in varchar2);
  function get_conn_str(p_conn_uid in dbcons.con_uid%type) return varchar2;
  procedure set_raddr_desc(
    p_session_remote_ip in RADDRSS.rem_addr%type,
    p_session_remote_desc in RADDRSS.addr_desc%type
  );
  procedure send_reg_card(
    p_usr_uid in varchar2,
    p_subact in varchar2,
    p_forceActivate in varchar2,
    p_biosystitle in varchar2,
    p_cnfrm_url in varchar2);
  procedure save_role(
    p_role_uid in varchar2,
    p_role_uid_new in varchar2,
    p_role_name in varchar2,
    p_role_is_sys in number,
    p_role_desc in varchar2);
  function usr_is_member_of_higher_odep(
    p_usr_uid in usrs.usr_uid%type,
    p_odep_uid in orgstruct.odep_uid%type) return number;
  function get_default_conn return varchar2;
  procedure ed_ws(
    p_odep_uid in orgstruct.odep_uid%type,
    p_usr_uid in usrs.usr_uid%type,
    p_odep_name in orgstruct.odep_name%type,
    p_odep_desc in orgstruct.odep_desc%type,
    p_biosystitle in varchar2
  );
  procedure detect_admins(p_usr_uid in varchar2, v_is_admin out number, v_is_wsadmin out number);
  procedure confirm_usr(p_usr_uid in usrs.usr_uid%type);
  procedure check_usr_exists(ppUsr in varchar2);
  procedure check_odep_exists(pODepName in varchar2);
  function usr_is_member_of_odep(
    p_usr_uid in usrs.usr_uid%type,
    p_odep_uid in orgstruct.odep_uid%type) return number;
  function usr_is_wsadmin(
    p_usr in usrs.usr_uid%type) return number;
  function usr_roles_ava_2add(
    p_usr_uid in varchar2,
    p_cur_usr in varchar2) return t_usrole_tbl pipelined;
  function get_tree(
    p_node_uid in orgstruct.odep_uid%type,
    p_usr_uid in usrs.usr_uid%type) return T_NODE_TBL pipelined;
  function usr_is_bioadmin(
    p_usr in usrs.usr_uid%type) return number;
  function get_odep_owner(
    p_odep_uid in varchar2) return varchar2;
  function get_usr(p_usr in varchar2)
    return t_usr_tbl pipelined;
  function reg_ws(
    p_usr_uid in usrs.usr_uid%type,
    p_usr_name in usrs.usr_name%type,
    p_usr_pwd in usrs.usr_pwd%type,
    p_odep_name in orgstruct.odep_name%type,
    p_odep_desc in orgstruct.odep_desc%type,
    p_usr_fam in usrs.usr_fam%type,
    p_usr_fname in usrs.usr_fname%type,
    p_usr_sname in usrs.usr_sname%type,
    p_email in usrs.email_addr%type,
    p_phone in usrs.usr_phone%type,
    p_sact in varchar2,
    p_forceActivate in varchar2,
    p_cnfrm_url in varchar2,
    p_biosystitle in varchar2
  ) return varchar2;
  function get_usr_list(p_usr_uid in varchar2) return t_usr_tbl pipelined;
  function ed_user(
    p_usr_uid in varchar2,
    p_usr_name in varchar2 default cs_dont_change_value_flag,
    p_usr_pwd in varchar2 default cs_dont_change_value_flag,
    p_con_uid in varchar2 default cs_dont_change_value_flag,
    p_odep_uid in varchar2 default cs_dont_change_value_flag,
    p_usr_fam in varchar2 default cs_dont_change_value_flag,
    p_usr_fname in varchar2 default cs_dont_change_value_flag,
    p_usr_sname in varchar2 default cs_dont_change_value_flag,
    p_email_addr in varchar2 default cs_dont_change_value_flag,
    p_usr_phone in varchar2 default cs_dont_change_value_flag,
                p_blocked in varchar2 default cs_dont_change_value_flag,
                p_confirmed in varchar2 default cs_dont_change_value_flag,
                p_garbaged in varchar2 default cs_dont_change_value_flag,
                p_extinfo in varchar2 default cs_dont_change_value_flag
  ) return varchar2;
END; -- Package spec
/


DROP PACKAGE BIOSYS.AI_DPBKP;

CREATE OR REPLACE PACKAGE BIOSYS.AI_DPBKP AS


  procedure exp_schema(p_schema_name in varchar2, p_exp_path in varchar2);
  procedure imp_schema(
    p_from_schema in varchar2, 
    p_from_tss in varchar2,
    p_from_file_name in varchar2, 
    p_from_path in varchar2,
    p_to_schema in varchar2, 
    p_to_tss in varchar2);
  function gen_dmp_file_name(p_schema_name in varchar2) return varchar2;
END;
/


DROP PACKAGE BIOSYS.AI_ORG;

CREATE OR REPLACE PACKAGE BIOSYS.AI_ORG
  IS

  subtype t_guid is varchar2(32);

  /*type T_ORGLOO_REC is record(
    ORG_UID orgs.org_uid%type,
    ORG_NAME orgs.org_name%type,
    OWNER_UID orgs.owner_uid%type
  );*/
  --type T_ORGLOO_TBL is table of T_ORGLOO_REC;

  procedure move_obj(
  	P_OBJ_UID in varchar2,
  	P_OBJ_TP  in varchar2,
  	P_ODEP_TO in orgstruct.odep_uid%type);
  function node_is_child_to(
    parent_node in varchar2,
    child_node in varchar2
    ) return number;
  procedure odep_set_owner(
    p_odep_uid in orgstruct.odep_uid%type,
    p_owner in usrs.usr_uid%type);
  procedure odep_save(
    p_odep_uid in orgstruct.odep_uid%type,
    p_odep_uid_new in out orgstruct.odep_uid%type,
    p_odep_prnt in orgstruct.odep_prnt%type,
    p_odep_name in orgstruct.odep_name%type,
    p_odep_desc in orgstruct.odep_desc%type,
    p_odep_uid_auto in orgstruct.odep_uid_auto%type
    );
  procedure odep_drop(
  	P_ODEP_UID in orgstruct.odep_uid%type);
END; -- Package spec
/


DROP PACKAGE BIOSYS.AI_SMTP;

CREATE OR REPLACE PACKAGE BIOSYS.ai_smtp AS




    PROCEDURE send (p_mailhost       IN VARCHAR2,
                    p_username       IN VARCHAR2,
                    p_password       IN VARCHAR2,
                    p_port           IN NUMBER,
                    p_sender         IN VARCHAR2,
                    p_recipient      IN VARCHAR2,
                    p_ccrecipient    IN VARCHAR2 default null,
                    p_bccrecipient   IN VARCHAR2 default null,
                    p_subject        IN VARCHAR2 default null,
                    p_message_text   IN CLOB default EMPTY_CLOB(),
                    p_message_html   IN CLOB default EMPTY_CLOB(),
                    p_encoding       IN VARCHAR2 default 'windows-1251',
                    p_filename       IN VARCHAR2 DEFAULT NULL,
                    p_binaryfile     IN BLOB DEFAULT EMPTY_BLOB()
                    );

    /*
    PROCEDURE send_html (p_mailhost       IN VARCHAR2,
                         p_username       IN VARCHAR2,
                         p_password       IN VARCHAR2,
                         p_port           IN NUMBER,
                         p_sender         IN VARCHAR2,
                         p_recipient      IN VARCHAR2,
                         p_ccrecipient    IN VARCHAR2,
                         p_bccrecipient   IN VARCHAR2,
                         p_subject        IN VARCHAR2,
                         p_message_text   IN CLOB,
                         p_message_html   IN CLOB,
                         p_encoding       IN VARCHAR2 default 'windows-1251');
    */

END;
/


DROP PACKAGE BIOSYS.AI_UTL;

CREATE OR REPLACE PACKAGE BIOSYS.ai_utl
  IS
   type T_VARCHAR_TBL is table of varchar2 (4000);

   type T_INLIST_REC is record (
      item   varchar2 (4000)
   );

   type T_INLIST_TBL is table of T_INLIST_REC;

  function pop_val_from_list1(
    vLIST in out nocopy CLOB,
    pDelimeter in varchar2 default ',')return CLOB;
  procedure push_val_to_list(
    vLIST in out nocopy varchar2,
    pVal in varchar2,
    pDelimeter in varchar2 default ',');
  function trans_list(pList in varchar2, pDelimeter in varchar2 default ',')
    return T_INLIST_TBL pipelined;
  function pop_last_val_from_list(
    vLIST in out nocopy VARCHAR2,
    pDelimeter in varchar2 default ',')return VARCHAR2;
  function item_in_list(p_item in varchar2, p_list in varchar2, p_delimeter in varchar2 default ',') return number;
  function get_word (pSource varchar2, pIndex number, pDelimeter varchar2 := '/')
      return varchar2;
  procedure sleep (pInterval in number);
  function sleep (pInterval in number)
    return number;
  procedure pars_tablename(full_tbl_name in varchar2, schm_name out varchar2, tbl_name out varchar2);
  function split_str(pStr in varchar2, pDelimeter in varchar2 default ',')
    return T_VARCHAR_TBL pipelined;
  function lists_is_equals(p_lst1 in varchar2, p_lst2 in varchar2, p_delimeter in varchar2) return number;
  procedure compare_lists(
    p_lst1 in varchar2, p_lst2 in varchar2, p_delimeter in varchar2,
    v_add_to_lst1 out varchar2, v_add_to_lst2 out varchar2);
  function pwd (nlength in number)
    return varchar2;
  function decrypt_string(input_string in varchar2, key_line in varchar2) return varchar2;
  function encrypt_string(input_string in varchar2, key_line in varchar2) return varchar2;
  function invert_list(pStr in varchar2, pDelimeter in varchar2 default ',')
    return varchar2;
  FUNCTION add_periods(pPeriod IN VARCHAR2, pMonths IN PLS_INTEGER) RETURN VARCHAR2;
  function age_calc(borndate in date) return  number;
  function bitor( x in number, y in number ) return number;
  function bitxor( x in number, y in number ) return number;
  function next_period(pPeriod in varchar2) return varchar2;
  FUNCTION num_to_word(vpr_num IN NUMBER) RETURN VARCHAR2;
  function pars_time(pTime in varchar2) return number;
  function period_name(pPeriod IN varchar2, pShort in number default 0) return varchar2;
  function period_name0( pPeriod in varchar2) return varchar2;
  function prev_period(pPeriod in varchar2) return varchar2;
  function time_between_str(pDtStart in date, pDtEnd in date)return varchar2;
  function vchar2num(pAttrValue in varchar2) return number;
    procedure pars_login(p_login in varchar2, v_usr_name out varchar2, v_usr_pwd out varchar2);

    FUNCTION add_days(pDate IN DATE, pDays IN PLS_INTEGER) RETURN DATE;

    function lists_has_common(p_lst1 in varchar2, p_lst2 in varchar2, p_delimeter in varchar2) return number; --RESULT_CACHE;

    function pop_val_from_list(
      vLIST in out nocopy varchar2,
      pDelimeter in varchar2 default ',')return varchar2;

    function extract_day(p_date date) return number;

END;
/


DROP PACKAGE BIOSYS.BIO_LOGIN;

CREATE OR REPLACE PACKAGE BIOSYS.bio_login
  IS

  /*
  Данный пакет предназначен для работы системы BioSys
  Является интерфейсом взаимодействия BioSys c RDBMS
  */
  subtype t_uid           is varchar2(32);
  subtype t_user_name     is varchar2(100);
  subtype t_usr_fio       is varchar2(300);
  subtype t_list          is varchar2(4000);
  subtype t_text          is varchar2(4000);
  subtype t_flag          is number(1);

  subtype t_ip            is varchar2(32);
  subtype t_host          is varchar2(100);
  subtype t_session_id    is varchar2(100);
  subtype t_client        is varchar2(300);
  subtype t_status        is varchar2(200);
  subtype t_login          is varchar2(100);

  csBIO_CONTEXT varchar2(32) := 'BIO_CONTEXT';
  type usr_rec is record(
    usr_uid                     t_uid,
    usr_name                    t_user_name,
    usr_fio_fam                 t_usr_fio,
    usr_fio_name                t_usr_fio,
    usr_fio_sname               t_usr_fio,
    reg_date                    date,
    usr_roles                   t_list,
    email_addr                  t_list,
    usr_phone                   t_list,
    odep_uid                    t_uid,
    odep_path                   t_list,
    odep_uid_path               t_list,
    odep_name                   t_text,
    odep_desc                   t_text,
    owner_uid                   t_uid,
    is_admin                    t_flag,
    is_wsadmin                  t_flag,
    extinfo                     t_text
  );
  type usr_tbl is table of usr_rec;

    function get_usr(p_login in t_login) return usr_tbl pipelined;

    procedure reg_connection(
      p_user in t_user_name,
      p_session_id in t_session_id,
      p_session_remote_ip in t_ip,
      p_session_remote_host in t_host,
      p_session_remote_client in t_client,
      p_status in t_status
    );

    procedure set_context_value(p_name in varchar2, p_value in varchar2);

    function get_context_value(p_name in varchar2) return varchar2;-- RESULT_CACHE;

END;
/


DROP PACKAGE BIOSYS.DDL$UTL;

CREATE OR REPLACE PACKAGE BIOSYS.DDL$UTL
  IS
  procedure reset_seq(seq_name in varchar2, tbl_name in varchar2, id_fld_name in varchar2);
END; -- Package spec
/


GRANT EXECUTE ON BIOSYS.AI_ADMIN TO GIVCADMIN;

GRANT EXECUTE ON BIOSYS.AI_SMTP TO GIVCADMIN;

GRANT EXECUTE ON BIOSYS.DDL$UTL TO GIVCADMIN;

GRANT EXECUTE ON BIOSYS.AI_UTL TO GIVC_ORG;

GRANT EXECUTE ON BIOSYS.AI_ORG TO PUBLIC;

GRANT EXECUTE ON BIOSYS.AI_UTL TO PUBLIC;

GRANT EXECUTE ON BIOSYS.BIO_LOGIN TO PUBLIC;
DROP PACKAGE BODY BIOSYS.AI_ADMIN;

CREATE OR REPLACE PACKAGE BODY BIOSYS.AI_ADMIN
IS
  csBIOSYS_NAMESPACE varchar2(32) := 'BIOSYS';
  csCURUSERNAME_ATTR varchar2(32) := 'CURUSERNAME';

  function get_default_conn return varchar2
  is
    vResult dbcons.con_uid%type;
  begin
    begin
      select a.con_uid into vResult
        from ai_dbs a
       where a.is_default = 1
         and rownum = 1;
    exception
      when no_data_found then
        vResult := null;
    end;
    if vResult is null then
      begin
        select a.con_uid into vResult
          from ai_dbs a
         where rownum = 1;
      exception
        when no_data_found then
          vResult := null;
      end;
    end if;
    return vResult;
  end;

  procedure check_usr_exists(ppUsr in varchar2)
  is
    vUserExists number(3);
  begin
    select count(1)
      into vUserExists
      from ai_usrs a
     where a.usr_name =  upper(ppUsr);

    if vUserExists > 0 then
      raise_application_error(-20100, 'Пользователь с именем "'||ppUsr||'" в системе зарегистрирован ранее.');
    end if;
  end;

  procedure check_odep_exists(pODepName in varchar2)
  is
    vOrgExists number(3);
  begin
    select count(1)
      into vOrgExists
      from AI_ORGSTRUCT a
     where upper(a.odep_name) =  upper(pODepName);


    if vOrgExists > 0 then
      raise_application_error(-20100, 'Организация с именем "'||pODepName||'" в системе зарегистрирована ранее.');
    end if;
  end;

  procedure send_reg_card(
    p_usr_uid in varchar2,
    p_subact in varchar2,
    p_forceActivate in varchar2,
    p_biosystitle in varchar2,
    p_cnfrm_url in varchar2)
  is
        ca_from_addr varchar2(100) := 'aw_gimckt@mail.ru';
    ca_srv varchar2(100) := 'smtp.mail.ru';
    ca_port varchar2(100) := '25';
    ca_user varchar2(100) := 'aw_gimckt';
    ca_pwd varchar2(100) := 'fkkflby';
    va_cnfrm_url varchar2(4000) := p_cnfrm_url;
    va_sendto varchar2(100);
    va_subject varchar2(4000);
    va_body varchar2(32000);
    va_err clob;
  begin
    va_cnfrm_url := replace(va_cnfrm_url, '#USR_UID#', p_usr_uid);
    get_reg_card(p_subact, p_forceActivate, p_biosystitle, p_usr_uid, va_cnfrm_url, va_sendto, va_subject, va_body);

    ai_smtp.send(
      p_mailhost=>ca_srv,
      p_username=>ca_user,
      p_password=>ca_pwd,
      p_port=>to_number(ca_port),
      p_sender=>ca_from_addr,
      p_recipient=>va_sendto,
      p_ccrecipient=>null,
      p_bccrecipient=>null,
      p_subject=>substr(trim(va_subject), 1, 1000),
      p_message_text=>va_body);
  exception
    when OTHERS then
    begin
      va_err := sqlerrm;
      insert into smtp$log
       values(sys_guid(), sysdate, ca_srv, ca_port, ca_user, ca_pwd,
              ca_from_addr, va_sendto, substr(trim(va_subject), 1, 1000), va_err);
    end;
  end;

  function reg_ws(
    p_usr_uid in usrs.usr_uid%type,
    p_usr_name in usrs.usr_name%type,
    p_usr_pwd in usrs.usr_pwd%type,
    p_odep_name in orgstruct.odep_name%type,
    p_odep_desc in orgstruct.odep_desc%type,
    p_usr_fam in usrs.usr_fam%type,
    p_usr_fname in usrs.usr_fname%type,
    p_usr_sname in usrs.usr_sname%type,
    p_email in usrs.email_addr%type,
    p_phone in usrs.usr_phone%type,
    p_sact in varchar2,
    p_forceActivate in varchar2,
    p_cnfrm_url in varchar2,
    p_biosystitle in varchar2
  ) return varchar2
  is
    vUserUID usrs.usr_uid%type := upper(p_usr_uid);
    vConnID dbcons.con_uid%type := get_default_conn;
    va_usr_name_old usrs.usr_name%type;
    vODepUID orgstruct.odep_uid%type := 'BIOSYS';
  begin
    if(vUserUID is not null)then
      dbms_output.put_line('rereg existing ODEP!');
      begin
        select usr_name
          into va_usr_name_old
          from ai_usrs a
         where a.usr_uid = vUserUID;
      exception
        when NO_DATA_FOUND then
          va_usr_name_old := null;
      end;
      dbms_output.put_line('va_usr_name_old:='||va_usr_name_old);
      if va_usr_name_old is null then
        raise_application_error(-20110, 'Ваша учетная запись удалена.');
      end if;
      if(p_usr_name <> va_usr_name_old) then
        check_usr_exists(p_usr_name);
      end if;
      select max(a.odep_uid) into vODepUID from ai_orgstruct a
       where a.owner_uid = vUserUID;
      dbms_output.put_line('vODepUID:='||vODepUID);
      if(vODepUID is not null) then
        --vODepUID := ai_org.orgsave(vOrgUID,substr(p_org_name, 1, 500),substr(p_org_desc, 1, 1000));
        dbms_output.put_line('before ai_org.odep_save. p_odep_name:='||p_odep_name);
        ai_org.odep_save(vODepUID, vODepUID, null, p_odep_name, p_odep_desc, 1);
        ai_org.odep_set_owner(vODepUID, vUserUID);
        dbms_output.put_line('odep_save done!');
      else
        null;
        /* Что делать??? */
      end if;
      update usrs a set
       (a.usr_name,
        a.usr_pwd,
        a.usr_fam, a.usr_fname, a.usr_sname,
        a.email_addr, a.usr_phone)=
      (select
        upper(substr(trim(p_usr_name), 1, 64)),
        decode(p_usr_pwd, 'pwd_is_not_changed', a.usr_pwd, substr(p_usr_pwd, 1, 32)),
        upper(substr(trim(p_usr_fam), 1, 100)), upper(substr(trim(p_usr_fname), 1, 100)), upper(substr(trim(p_usr_sname), 1, 100)),
        substr(trim(p_email), 1, 100), substr(trim(p_phone), 1, 12)
      from dual)
      where usr_uid = vUserUID;
    else
      dbms_output.put_line('reg new ODEP!');
      check_usr_exists(p_usr_name);

      vUserUID := upper(sys_guid());
      vConnID := get_default_conn;
      --vOrgUID := ai_org.orgsave(null,substr(p_org_name, 1, 500),substr(p_org_desc, 1, 1000));
      dbms_output.put_line('before ai_org.odep_save. p_odep_name:='||p_odep_name);
      vODepUID:=null;
      ai_org.odep_save(null, vODepUID, null, p_odep_name, p_odep_desc, 1);
      dbms_output.put_line('odep_save done! vODepUID := '||vODepUID);
      insert into usrs(
        usr_uid, odep_uid,
        con_uid, usr_name, usr_pwd,
        usr_fam, usr_fname, usr_sname, reg_date, blocked,
        email_addr, usr_phone,
        confirmed)
      values(
        vUserUID, vODepUID,
        vConnID, upper(substr(trim(p_usr_name), 1, 64)), substr(p_usr_pwd, 1, 32),
        upper(substr(trim(p_usr_fam), 1, 100)), upper(substr(trim(p_usr_fname), 1, 100)), upper(substr(trim(p_usr_sname), 1, 100)), sysdate, 0,
        substr(trim(p_email), 1, 100), substr(trim(p_phone), 1, 12),
        decode(p_forceActivate, '1', 1, 0)
      );
      dbms_output.put_line('inserted: '||sql%rowcount);
      ai_org.odep_set_owner(vODepUID,vUserUID);
      dbms_output.put_line('odep_set_owner('||vODepUID||','||vUserUID||') - done.');
    end if;
    send_reg_card(vUserUID, p_sact, p_forceActivate, p_biosystitle, p_cnfrm_url);
    return vUserUID;
  end;

  procedure ed_ws(
    p_odep_uid in orgstruct.odep_uid%type,
    p_usr_uid in usrs.usr_uid%type,
    p_odep_name in orgstruct.odep_name%type,
    p_odep_desc in orgstruct.odep_desc%type,
    p_biosystitle in varchar2
  )
  is
    vODepUID orgstruct.odep_uid%type := p_odep_uid;
  begin
    if(vODepUID is not null) then
      vODepUID := null;
      ai_org.odep_save(vODepUID, vODepUID, null, p_odep_name,p_odep_desc, 1);
      ai_org.odep_set_owner(vODepUID, p_usr_uid);
    end if;
    send_reg_card(p_usr_uid, 'edit', 1, p_biosystitle, null);

  end;

  function ed_user(
    p_usr_uid in varchar2,
    p_usr_name in varchar2 default cs_dont_change_value_flag,
    p_usr_pwd in varchar2 default cs_dont_change_value_flag,
    p_con_uid in varchar2 default cs_dont_change_value_flag,
    p_odep_uid in varchar2 default cs_dont_change_value_flag,
    p_usr_fam in varchar2 default cs_dont_change_value_flag,
    p_usr_fname in varchar2 default cs_dont_change_value_flag,
    p_usr_sname in varchar2 default cs_dont_change_value_flag,
    p_email_addr in varchar2 default cs_dont_change_value_flag,
    p_usr_phone in varchar2 default cs_dont_change_value_flag,
                p_blocked in varchar2 default cs_dont_change_value_flag,
                p_confirmed in varchar2 default cs_dont_change_value_flag,
                p_garbaged in varchar2 default cs_dont_change_value_flag,
                p_extinfo in varchar2 default cs_dont_change_value_flag
  ) return varchar2
  is
    vConnID dbcons.con_uid%type := p_con_uid;
    vOldConnID dbcons.con_uid%type;
    vUserUID usrs.usr_uid%type := upper(p_usr_uid);
    va_usr_name_old usrs.usr_name%type;
    key_line varchar2(100) := sys_context('BIO_CONTEXT', 'key_line');
    --vODEP_UID orgstruct.odep_uid%type := p_odep_uid;
  begin
    /*if vODEP_UID is null then
      vODEP_UID := ai_org.get_root_odep(p_odep_uid);
    end if;*/
    if(vUserUID is not null)then
      begin
        select a.usr_name, a.con_uid
          into va_usr_name_old, vOldConnID
          from ai_usrs a
         where a.usr_uid = vUserUID;
      exception
        when NO_DATA_FOUND then
          va_usr_name_old := null;
      end;
      if va_usr_name_old is null then
        raise_application_error(-20110, 'Ваша учетная запись удалена.');
      end if;
      if(p_usr_name <> va_usr_name_old) then
        check_usr_exists(p_usr_name);
      end if;
      update usrs a set
       (a.odep_uid,
        a.con_uid,
        a.usr_name,
        a.usr_pwd,
        a.usr_fam,
        a.usr_fname,
        a.usr_sname,
        a.reg_date,
        a.email_addr,
        a.usr_phone,
        a.blocked,
        a.confirmed,
        a.garbaged,
        a.extinfo)=
      (select
        decode(p_odep_uid, cs_dont_change_value_flag, a.odep_uid, p_odep_uid),
        decode(vConnID, cs_dont_change_value_flag, a.con_uid, nvl(vConnID, vOldConnID)),
        decode(p_usr_name, cs_dont_change_value_flag, a.usr_name, upper(substr(trim(p_usr_name), 1, 64))),
        decode(p_usr_pwd, cs_dont_change_value_flag, a.usr_pwd, ai_utl.encrypt_string(substr(p_usr_pwd, 1, 32), key_line)),
        decode(p_usr_fam, cs_dont_change_value_flag, a.usr_fam, upper(substr(trim(p_usr_fam), 1, 100))),
        decode(p_usr_fname, cs_dont_change_value_flag, a.usr_fname, upper(substr(trim(p_usr_fname), 1, 100))),
        decode(p_usr_sname, cs_dont_change_value_flag, a.usr_sname, upper(substr(trim(p_usr_sname), 1, 100))),
        sysdate,
        decode(p_email_addr, cs_dont_change_value_flag, a.email_addr, substr(trim(p_email_addr), 1, 100)),
        decode(p_usr_phone, cs_dont_change_value_flag, a.usr_phone, substr(trim(p_usr_phone), 1, 12)),
        decode(p_blocked, cs_dont_change_value_flag, a.blocked, nvl(to_number(p_blocked), 0)),
        decode(p_confirmed, cs_dont_change_value_flag, a.confirmed, nvl(to_number(p_confirmed), 0)),
        decode(p_garbaged, cs_dont_change_value_flag, a.blocked, nvl(to_number(p_garbaged), 0)),
        decode(p_extinfo, cs_dont_change_value_flag, a.extinfo, nvl(p_extinfo, 0))
      from dual)
      where usr_uid = vUserUID;
    else
      if((vConnID is null)or(vConnID = cs_dont_change_value_flag))then
        vConnID := get_default_conn;
      end if;
      check_usr_exists(p_usr_name);
      vUserUID := upper(sys_guid());
      insert into usrs(
        usr_uid, odep_uid,
        con_uid, usr_name, usr_pwd,
        usr_fam, usr_fname, usr_sname, reg_date,
        email_addr, usr_phone,
        blocked, confirmed, garbaged, extinfo)
      values(
        vUserUID,
        decode(p_odep_uid, cs_dont_change_value_flag, null, p_odep_uid),
        vConnID,
        decode(p_usr_name, cs_dont_change_value_flag, null, upper(substr(trim(p_usr_name), 1, 64))),
        decode(p_usr_pwd, cs_dont_change_value_flag, null, ai_utl.encrypt_string(substr(p_usr_pwd, 1, 32), key_line)),
        decode(p_usr_fam, cs_dont_change_value_flag, null, upper(substr(trim(p_usr_fam), 1, 100))),
        decode(p_usr_fname, cs_dont_change_value_flag, null, upper(substr(trim(p_usr_fname), 1, 100))),
        decode(p_usr_sname, cs_dont_change_value_flag, null, upper(substr(trim(p_usr_sname), 1, 100))),
        sysdate,
        decode(p_email_addr, cs_dont_change_value_flag, null, substr(trim(p_email_addr), 1, 100)),
        decode(p_usr_phone, cs_dont_change_value_flag, null, substr(trim(p_usr_phone), 1, 12)),
        decode(p_blocked, cs_dont_change_value_flag, 0, nvl(to_number(p_blocked), 0)),
        decode(p_confirmed, cs_dont_change_value_flag, 0, nvl(to_number(p_confirmed), 0)),
        decode(p_garbaged, cs_dont_change_value_flag, 0, nvl(to_number(p_garbaged), 0)),
        decode(p_extinfo, cs_dont_change_value_flag, null, nvl(p_extinfo, 0))
      );
    end if;
    return vUserUID;
  end;

  function usr_roles(
    pUsrUID in usrs.usr_uid%type,
    pInvert in number default 0
    ) return t_usrole_tbl pipelined
  is
    vOwndOrgs pls_integer;
    vRslt t_usrole_rec;
    vContinue boolean;
    type t_cur is ref cursor return t_usrole_rec;
    cur t_cur;
  begin
    select count(1) into vOwndOrgs
      from orgstruct a where a.owner_uid = upper(pUsrUID);

    if nvl(pInvert, 0) = 1 then
      if vOwndOrgs > 0 then
        vOwndOrgs := 0;
      end if;
      open cur for
        select distinct a.*, pUsrUID as usr_uid from uroles a
         where not exists(select 1 from usrrles b where b.role_uid = a.role_uid and b.usr_uid = pUsrUID)
            or a.role_uid = csWSADMIN_ROLE_UID;
    else
      open cur for
        select distinct a.*, pUsrUID as usr_uid from uroles a
         where exists(select 1 from usrrles b where b.role_uid = a.role_uid and b.usr_uid = pUsrUID)
            or a.role_uid = csWSADMIN_ROLE_UID;
    end if;
    loop
      fetch cur into vRslt;
      exit when cur%notfound;
      vContinue := true;
      if (vRslt.role_uid = csWSADMIN_ROLE_UID) then
        vContinue := ((vRslt.role_uid = csWSADMIN_ROLE_UID) and (vOwndOrgs > 0));
      end if;

      if vContinue then
        pipe row (vRslt);
      end if;
    end loop;
    close cur;
  end;

  function get_usr_roles(p_usr_uid in usrs.usr_uid%type) return varchar2
  is
    vRslt varchar2(4000);
  begin
    vRslt := null;
    for c in (select role_uid from table(usr_roles(p_usr_uid)) where usr_uid = p_usr_uid) loop
      if vRslt is null then
        vRslt := c.role_uid;
      else
        vRslt := vRslt || ';' || c.role_uid;
      end if;
    end loop;
    return vRslt;
  end;

  function usr_has_role(
    pUsrUID in usrs.usr_uid%type,
    pRole in uroles.role_uid%type) return number
  is
    vRslt number(1) := 0;
  begin
    select count(1) into vRslt
     from table(usr_roles(pUsrUID)) a
    where a.usr_uid = upper(pUsrUID)
      and a.role_uid = upper(pRole);
    return vRslt;
  end;

  procedure confirm_usr(p_usr_uid in usrs.usr_uid%type)
  is
  begin
    update usrs set confirmed = 1
     where usr_uid = p_usr_uid;
  end;

  procedure detect_admins(p_usr_uid in varchar2, v_is_admin out number, v_is_wsadmin out number)
  is
  begin
    v_is_admin := usr_is_bioadmin(p_usr_uid);--(case when usr_has_role(p_usr_uid, csADMIN_ROLE_UID) > 0 then 1 else 0 end);
    v_is_wsadmin := usr_is_wsadmin(p_usr_uid); --(case when usr_has_role(p_usr_uid, csWSADMIN_ROLE_UID) > 0 then 1 else 0 end);
  end;

  function get_usr_uid(p_usr in varchar2)
    return varchar2
  is
    vResult usrs.usr_uid%type;
  begin
    select usr_uid into vResult
     from usrs
    where usr_uid = upper(p_usr) or usr_name = upper(p_usr);
    return vResult;
  exception
    when NO_DATA_FOUND then
      return null;
  end;

  function get_usr(p_usr in varchar2)
    return t_usr_tbl pipelined
  is
    type t_cur is ref cursor return t_usr_rec;
    vRec t_usr_rec;
    cur t_cur;
    v_usr_uid usrs.usr_uid%type := get_usr_uid(p_usr);
  begin
    open cur for
      SELECT a.* FROM table(get_usr_list(v_usr_uid)) a
       WHERE a.usr_uid = v_usr_uid;
    fetch cur into vRec;
    if not cur%notfound then
      pipe row(vRec);
    end if;
    close cur;
  end;

  function get_conn_str(p_conn_uid in dbcons.con_uid%type) return varchar2
  is
    vRslt varchar2(4000);
  begin
    select max(a.con_str) into vRslt
      from ai_dbs a where a.con_uid = p_conn_uid;
    return vRslt;
  end;

  procedure set_raddr_desc(
    p_session_remote_ip in RADDRSS.rem_addr%type,
    p_session_remote_desc in RADDRSS.addr_desc%type
  )
  is
  begin
    update RADDRSS set addr_desc = p_session_remote_desc
     where rem_addr = p_session_remote_ip;
  end;

/*  procedure reg_connection(
    p_user in USRIN$LOG.usr_name%type,
    p_session_id in USRIN$LOG.session_id%type,
    p_session_remote_ip in RADDRSS.rem_addr%type,
    p_session_remote_host in RADDRSS.rem_host%type,
    p_session_remote_client in USRIN$LOG.rem_client%type,
    p_status in USRIN$LOG.astatus%type
  )
  is
    vNextNumOfIn USRIN$LOG.usrin_num%type;
    vUser USRIN$LOG.usr_name%type := upper(p_user);
    vCurSessionID USRIN$LOG.session_id%type := upper(substr(p_session_id, 1, 32));
  begin
    reg_RAddr(p_Session_Remote_IP, p_Session_Remote_Host);
    select seq_usrin_log.NEXTVAL into vNextNumOfIn from dual;
    insert into USRIN$LOG(rem_addr,usrin_num,usr_name,session_id,rem_client,astatus,usrin_date)
    values(substr(p_Session_Remote_IP, 1, 32), vNextNumOfIn, substr(nvl(vUser, '<noname>'), 1, 64), vCurSessionID, substr(nvl(p_Session_Remote_Client, '<не''что>'), 1, 1000), substr(p_Status, 1, 200), sysdate);
    --dbms_session.set_context (
    --           namespace => csBIOSYS_NAMESPACE,
    --           attribute => csCURUSERNAME_ATTR,
    --           value     => vUser,
    --           client_id => vCurSessionID);
    kill_old_usr(vUser);
  end;
*/
  procedure get_reg_card(
    p_act in varchar2,
    p_forceactivate in varchar2,
    p_biosystitle in varchar2,
    p_useruid in varchar2,
    p_cnfrmurl in varchar2,
    v_sendto out varchar2, v_subject out varchar2, v_body out varchar2)
  is
    csCR varchar2(2) := chr(13)||chr(10);
    csTAB varchar2(1) := chr(9);
    ciChrValTab pls_integer := 30;
    csTAB_FILL_CHAR char(1) := '.';
  begin
    select a.email_addr,
           decode(p_Act, 'reg', 'Регистрационная карта в системе '||p_BioSysTitle, 'Учетная карта рабочего пространства системы '||p_BioSysTitle),
           decode(p_Act, 'reg', 'Вы зарегистрировались в системе ', 'Вы изменили учетные данные в системе ')||p_BioSysTitle||'.'||csCR||
           decode(p_Act, 'reg', 'Ваши регистрационные данные:', 'Ваши новые учетные данные:')||csCR||
           rpad(csTAB||'Имя пользователя', ciChrValTab, csTAB_FILL_CHAR)||        ' '||a.usr_name||csCR||
           rpad(csTAB||'Пароль', ciChrValTab, csTAB_FILL_CHAR)||                  ' '||a.usr_pwd||csCR||
           rpad(csTAB||'Фамилия', ciChrValTab, csTAB_FILL_CHAR)||                 ' '||a.usr_fam||csCR||
           rpad(csTAB||'Имя', ciChrValTab, csTAB_FILL_CHAR)||                     ' '||a.usr_fname||csCR||
           rpad(csTAB||'Отчество', ciChrValTab, csTAB_FILL_CHAR)||                ' '||a.usr_sname||csCR||
           rpad(csTAB||'Подразделение', ciChrValTab, csTAB_FILL_CHAR)||           ' '||a.odep_name||csCR||
           rpad(csTAB||'Адрес эл. почты', ciChrValTab, csTAB_FILL_CHAR)||         ' '||a.email_addr||csCR||
           rpad(csTAB||'Телефон', ciChrValTab, csTAB_FILL_CHAR)||                 ' '||a.usr_phone||csCR||csCR||
           rpad(csTAB||'Дата и время регистрации', ciChrValTab, csTAB_FILL_CHAR)||' '||to_char(a.reg_date, 'DD.MM.YYYY HH24:MI:SS')||csCR||
           decode(p_forceActivate, '1', '',
             decode(p_Act, 'reg', 'Для окончания регистрации перейдите по след. ссылке: '||p_CnfrmURL||csCR||
             'Внимание! Если вы не перейдете по вышеуказанной ссылке ваша учетная запись не будет активирована.', null))
      into v_SendTo, v_Subject, v_Body
      from table(get_usr(upper(p_UserUID))) a;
  exception
    when NO_DATA_FOUND then
    begin
      v_SendTo := null;
      v_Subject := null;
      v_Body := null;
    end;
  end;

  /*function pop_val_from_list(
    vLIST in out nocopy VARCHAR2,
    pDelimeter in varchar2 default ',')return VARCHAR2
  is
    rslt VARCHAR2(320);
    vCommaPos integer;
  begin
    rslt := null;
    if length(vLIST) > 0  then
        vCommaPos := instr(vLIST, pDelimeter, 1);
        if vCommaPos > 0 then
          rslt := substr(vLIST, 1, vCommaPos-1);
          vLIST := substr(vLIST, vCommaPos+1);
        else
          rslt := vLIST;
          vLIST := '';
        end if;
    end if;
    return rslt;
  end;*/

  procedure set_usr_roles(
    p_usr_uid in varchar2,
    p_roles in varchar2)
  is
    vROLES varchar2(32000) := p_roles;
    vROLE varchar2(100);
  begin
    --raise_application_error(-20000, P_FA_USR_UID||': '||P_FA_ROLES);
    if(nvl(vROLES, 'null-value') <> 'unchanged-value')then
      delete from usrrles a where (a.usr_uid = upper(P_USR_UID)) and (a.role_uid not in (csADMIN_ROLE_UID));
      while vROLES is not null loop
        vROLE := ai_utl.pop_val_from_list(vROLES, ',');
        insert into usrrles(role_uid,usr_uid)
          values(vROLE, P_USR_UID);
      end loop;
    end if;
  end;

  procedure save_role(
    p_role_uid in varchar2,
    p_role_uid_new in varchar2,
    p_role_name in varchar2,
    p_role_is_sys in number,
    p_role_desc in varchar2)
  is
    procedure appendRole(pp_uid in varchar2)
    is
      vExists pls_integer;
    begin
      select count(1) into vExists from uroles a
      where a.role_uid = pp_uid;
      if vExists = 0 then
        insert into UROLES(ROLE_UID, ROLE_NAME, ROLE_IS_SYS, ROLE_DESC)
        values(pp_uid, p_role_name, nvl(p_role_is_sys, 0), p_role_desc);
      else
        raise_application_error(-20000, 'Роль с ID '||pp_uid||' уже существует в системе.');
      end if;
    end;
  begin
    if((P_ROLE_UID is not null)and(P_ROLE_UID_NEW is not null))then
      if(P_ROLE_UID <> P_ROLE_UID_NEW)then
        appendRole(P_ROLE_UID_NEW);
        update USRRLES a set a.role_uid = P_ROLE_UID_NEW
         where a.role_uid = P_ROLE_UID;
        delete from UROLES a where a.role_uid = P_ROLE_UID;
      else
        update UROLES a set
          a.role_name = p_role_name,
          a.role_is_sys = nvl(p_role_is_sys, 0),
          a.role_desc = p_role_desc
         where a.role_uid = p_role_uid;
      end if;
    else
      if((P_ROLE_UID is null)and(P_ROLE_UID_NEW is not null))then
        appendRole(P_ROLE_UID_NEW);
      end if;
    end if;
  end;

  function get_usr_list(p_usr_uid in varchar2) return t_usr_tbl pipelined
  is
    v_is_admin number(1);
    v_is_wsadmin number(1);
    vRslt t_usr_rec;
    type t_cur is ref cursor return t_usr_rec;
    cur t_cur;
    single_usr boolean := false;
    key_line varchar2(100) := sys_context('BIO_CONTEXT', 'KEY_LINE');
  begin
    dbms_output.put_line('p_usr_uid:'||p_usr_uid);
    detect_admins(p_usr_uid, v_is_admin, v_is_wsadmin);
    dbms_output.put_line('v_is_admin:'||v_is_admin||', v_is_wsadmin:'||v_is_wsadmin);
    if (v_is_admin = 1) or (v_is_wsadmin = 1) then
      open cur for
        SELECT a.* FROM ai_usrs_edlist a;
    else
      open cur for
        SELECT a.* FROM ai_usrs_edlist a
         WHERE a.usr_uid = p_usr_uid;
      single_usr := true;
    end if;
    loop
      fetch cur into vRslt;
      exit when cur%notfound;
      vRslt.usr_roles:=get_usr_roles(vRslt.usr_uid);
      vRslt.owner_uid:=get_odep_owner(vRslt.odep_uid);
      vRslt.usr_pwd:=ai_utl.decrypt_string(vRslt.usr_pwd,key_line);
      detect_admins(vRslt.usr_uid, vRslt.is_admin, vRslt.is_wsadmin);
      if single_usr then
        pipe row(vRslt);
      elsif (v_is_admin = 1) or ((v_is_wsadmin = 1) and (vRslt.owner_uid = p_usr_uid)) then
        pipe row(vRslt);
      end if;
    end loop;
    close cur;
  end;

  function usr_roles_ava_2add(
    p_usr_uid in varchar2,
    p_cur_usr in varchar2) return t_usrole_tbl pipelined
  is
    vCurUsrIsAdmin boolean := false;
    --vCurUsrIsDev boolean := false;
    vContinue boolean;
    vRslt t_usrole_rec;
    type t_cur is ref cursor return t_usrole_rec;
    cur t_cur;
  begin
    vCurUsrIsAdmin := usr_has_role(p_cur_usr, csADMIN_ROLE_UID) > 0;
    --vCurUsrIsDev := usrHasRole(PA_CUR_USR, csWSADMIN_ROLE_UID) > 0;
                open cur for select a.*
                           from table(usr_roles(p_usr_uid, 1)) a
                     where a.role_is_sys = 0;
    loop
      fetch cur into vRslt;
      exit when cur%notfound;
      vContinue := true/*vCurUsrIsAdmin or (vRslt.role_uid <> csADMIN_ROLE_UID)*/;
      --if vContinue and not vCurUsrIsAdmin then
      --  vContinue := vCurUsrIsDev or (vRslt.role_uid <> 'DEBUGERS');
      --end if;
      if vContinue then
        pipe row(vRslt);
      end if;
    end loop;
  end;

  function usr_is_wsadmin(
    p_usr in usrs.usr_uid%type) return number
  is
    v_owened_odep orgstruct.odep_uid%type;
    v_odep_uid orgstruct.odep_uid%type;
    v_usr_uid orgstruct.owner_uid%type;
    v_cnt pls_integer;
  begin
    select max(a.odep_uid) into v_odep_uid
      from usrs a where a.usr_uid = upper(p_usr);

    select count(1) into v_cnt
      from orgstruct a
      where a.owner_uid = upper(p_usr)
        and a.odep_uid = upper(v_odep_uid);

    --v_odep_uid := ai_org.get_root_odep(v_odep_uid, p_usr);

    if v_cnt > 0 then
      return 1;
    else
      return 0;
    end if;
  end;
  function usr_is_bioadmin(
    p_usr in usrs.usr_uid%type) return number
  is
  begin
    return (case when usr_has_role(p_usr, csADMIN_ROLE_UID) > 0 then 1 else 0 end);
  end;

  function get_usr_caption(
    p_usr_name in varchar2,
    p_usr_fam in varchar2,
    p_usr_fname in varchar2,
    p_usr_sname in varchar2
    ) return varchar2
  is
  begin
    return '['||p_usr_name||'] - '||trim(trim(p_usr_fam)||' '||trim(p_usr_fname)||' '||trim(p_usr_sname));
  end;

  function get_org_node_det(
    p_node_uid in orgstruct.odep_uid%type,
    p_cur_usr in usrs.usr_uid%type) return T_NODE_DET_TBL pipelined
  is
    vRec T_NODE_DET_REC;
    vCurUsrIsAdmin boolean := false;
    vCurUsrIsWsAdmin boolean := false;
  begin
    dbms_output.put_line(' p_cur_usr := '||p_cur_usr);
    vCurUsrIsAdmin := usr_has_role(p_cur_usr, csADMIN_ROLE_UID) > 0;
    vCurUsrIsWsAdmin := usr_is_wsadmin(p_cur_usr) > 0;
    dbms_output.put_line(' vCurUsrIsAdmin := '||case when vCurUsrIsAdmin then 1 else 0 end);
    dbms_output.put_line(' vCurUsrIsWsAdmin := '||case when vCurUsrIsWsAdmin then 1 else 0 end);
    --vRec.org_uid := p_org_uid;
    if vCurUsrIsAdmin or vCurUsrIsWsAdmin then
      for c in (select a.f_uid, a.f_text
                  from table(get_tree(p_node_uid, p_cur_usr)) a)
      loop
        --if(upper(p_node_uid) <> upper(c.f_uid))then
          vRec.f_uid := c.f_uid;
          vRec.f_text := c.f_text;
          vRec.f_icon := '1000';
          vRec.f_extinfo := null;
          pipe row(vRec);
        --end if;
      end loop;
      for c in (select a.usr_uid as f_uid,
                       get_usr_caption(a.usr_name, a.usr_fam, a.usr_fname, a.usr_sname) as f_text,
                       a.odep_uid,
                       a.confirmed, a.blocked, a.garbaged, a.extinfo
                  from table(get_usr_list(p_cur_usr)) a)
      loop
        if(c.odep_uid = upper(p_node_uid)) then
          vRec.f_uid := c.f_uid;
          vRec.f_text := c.f_text;
          vRec.f_icon := '0'||trim(to_char(c.confirmed))||trim(to_char(c.blocked))||trim(to_char(c.garbaged));
          vRec.f_extinfo := c.extinfo;
          pipe row(vRec);
        end if;
      end loop;
    end if;
  end;

  function usr_is_member_of_odep(
    p_usr_uid in usrs.usr_uid%type,
    p_odep_uid in orgstruct.odep_uid%type) return number
  is
    vUsrODEP_UID orgstruct.odep_uid%type;
    vCheckChildNode pls_integer;
  begin
    select max(a.odep_uid) into vUsrODEP_UID from ai_usrs a
     where a.usr_uid = upper(p_usr_uid);
    if upper(vUsrODEP_UID) = upper(p_odep_uid) then
      return 1;
    else
      vCheckChildNode := ai_org.node_is_child_to(p_odep_uid, vUsrODEP_UID);
      if vCheckChildNode > 0 then
        return 1;
      else
        return 0;
      end if;
    end if;
  end;

  function usr_is_member_of_higher_odep(
    p_usr_uid in usrs.usr_uid%type,
    p_odep_uid in orgstruct.odep_uid%type) return number
  is
    vUsrODEP_UID orgstruct.odep_uid%type;
    vCheckChildNode pls_integer;
  begin
    select max(a.odep_uid) into vUsrODEP_UID from ai_usrs a
     where a.usr_uid = upper(p_usr_uid);
    if upper(vUsrODEP_UID) = upper(p_odep_uid) then
      return 0;
    else
      vCheckChildNode := ai_org.node_is_child_to(vUsrODEP_UID, p_odep_uid);
      if vCheckChildNode > 0 then
        return 1;
      else
        return 0;
      end if;
    end if;
  end;

  function get_tree(
    p_node_uid in orgstruct.odep_uid%type,
    p_usr_uid in usrs.usr_uid%type) return T_NODE_TBL pipelined
  is
    v_node_prnt ai_orgstruct.odep_uid%type := p_node_uid;
    --v_node_of_usr ai_orgstruct.odep_uid%type;
    vRec T_NODE_REC;
    vChldCnt pls_integer;
    --vStartFromRoot boolean := p_node_uid is null;

    c ai_orgstruct%rowtype;
    type t_cur is ref cursor return ai_orgstruct%rowtype;
    cur t_cur;
  begin
    if v_node_prnt is null then
      select a.odep_prnt_conn into v_node_prnt
        from ai_orgstruct a
       where exists (select 1
                       from usrs b
                      where b.usr_uid = p_usr_uid
                        and b.odep_uid = a.odep_uid);
    end if;
    /*for c in (select
                a.odep_uid,
                a.odep_prnt,
                a.odep_name,
                a.odep_desc,
                a.odep_uid_auto,
                a.owner_uid,
                a.f_leaf
                 from ai_orgstruct a
                start with a.odep_uid = v_node_uid_start
               connect by prior  a.odep_uid = a.odep_prnt_conn
                 order by odep_name)
    loop
      if vStartFromRoot or (upper(c.odep_uid) <> upper(v_node_uid_start)) then
        vRec.f_uid  := c.odep_uid;
        vRec.f_parent_uid := c.odep_prnt;
        vRec.f_text := c.odep_name;
        vRec.f_cls  := 'folder';
        vRec.f_icon  := 'folder_crystal.png';
        select count(1) into vChldCnt from ai_orgstruct a
         where a.odep_prnt = vRec.f_uid;
        vRec.f_leaf := (case when vChldCnt > 0 then 1 else 0 end);
        pipe row(vRec);
      end if;
    end loop;*/

    if v_node_prnt is null then
      dbms_output.put_line(' v_node_prnt is null');
      open cur for
        select * from ai_orgstruct o
         where o.owner_uid = p_usr_uid;
    else
      dbms_output.put_line(' v_node_prnt = "'||v_node_prnt||'"');
      open cur for
        select * from ai_orgstruct o
         where exists(select 1 from ai_orgstruct a
                       where a.odep_uid = o.odep_prnt
                         --and
                       start with a.odep_uid = v_node_prnt
                     connect by prior  a.odep_uid = a.odep_prnt_conn)
           and o.odep_prnt = v_node_prnt
           --and ((usr_is_bioadmin(p_usr_uid) = 1) or (o.owner_uid = upper(p_usr_uid)))
         order by odep_name;
        /*select * from ai_orgstruct o
         where o.odep_prnt = v_node_prnt
         order by odep_name;*/
    end if;
    loop
      fetch cur into c;
      exit when cur%notfound;

        vRec.f_uid  := c.odep_uid;
        vRec.f_parent_uid := c.odep_prnt;
        vRec.f_text := c.odep_name;
        vRec.f_cls  := 'folder';
        vRec.f_icon  := 'folder_crystal.png';
        select count(1) into vRec.f_count from ai_orgstruct a
         where a.odep_prnt = vRec.f_uid;

      pipe row (vRec);
    end loop;
    close cur;

  end;

  function get_odep_owner(
    p_odep_uid in varchar2) return varchar2
  is
    vResult varchar2(32);
  begin
    select owner_uid into vResult from
      (SELECT
        rownum as rnum,
        e.owner_uid,
        level as lvl
        FROM orgstruct e
        where e.owner_uid is not null
        start with e.odep_uid = p_odep_uid
        connect by prior e.odep_prnt = e.odep_uid
        order by lvl)
        where rnum = 1;
    return vResult;
  exception
    when NO_DATA_FOUND then
      return null;
  end;


end;
/


DROP PACKAGE BODY BIOSYS.AI_DPBKP;

CREATE OR REPLACE PACKAGE BODY BIOSYS.AI_DPBKP AS

  csOraDir4ExpImp varchar2(100) := 'DIR_DPUMP_BIOEXPIMP';
  
  function gen_dmp_file_name(p_schema_name in varchar2) return varchar2
  is
    v_db_domain varchar2(100) := null;
    v_db_name varchar2(100) := null;
    v_host_name varchar2(100) := null;
  begin
    select sys_context('USERENV', 'DB_DOMAIN') into v_db_domain from dual;
    select sys_context('USERENV', 'DB_NAME') into v_db_name from dual;
    --select sys_context('USERENV', 'SERVER_HOST') into v_host_name from dual; 
    return replace(upper((case when v_host_name is null then null else v_host_name||'.' end)||
      v_db_name||'.'||(case when v_db_domain is null then null else v_db_domain||'.' end)||
      p_schema_name||'_'||to_char(sysdate, 'YYYYMMDD_HH24MISS')||'_DTPMP.DMP'), '$', '_');
  end;

  function open_dtpump_job(
    p_job_name in varchar2,
    p_job_mode in varchar2,
    p_job_owner in varchar2,
    p_operation in varchar2) return number
  is
    csJobOwner varchar2(100):= nvl(p_job_owner, user);
    csJobName varchar2(100):= p_job_name;
    vExistsStatus     varchar2(100);
    result            number;
  begin
    begin
      select a.state into vExistsStatus
       from SYS.DBA_DATAPUMP_JOBS a
       where a.owner_name = csJobOwner
         and a.job_name = csJobName;
    exception
      when NO_DATA_FOUND then
        vExistsStatus:='NOT_EXISTS';
    end;
    dbms_output.put_line('Status of job('||csJobName||') : '||vExistsStatus);
    if vExistsStatus in ('UNDEFINED', 'NOT RUNNING') then
      execute immediate 'drop table '||csJobName;
      vExistsStatus:='NOT_EXISTS';
    end if;

    if vExistsStatus in ('NOT_EXISTS') then
      dbms_output.put_line('Open job('||csJobName||') ... ');
      result := DBMS_DATAPUMP.open(
        operation   => p_operation,
        job_mode    => p_job_mode,
        remote_link => NULL,
        job_name    => csJobName,
        version     => 'LATEST');
      dbms_output.put_line('Job('||csJobName||') opened. ');
    else
      dbms_output.put_line('Attach job('||csJobName||') ... ');
      result := DBMS_DATAPUMP.attach(csJobName,csJobOwner);
      dbms_output.put_line('Job('||csJobName||') attached. ');
    end if;
    dbms_output.put_line('Handle of job('||csJobName||') : '||result);
    return result;
  end;

  procedure init_ora_directory(p_name in varchar2, p_path in varchar2)
  is
  begin
    execute immediate 'CREATE OR REPLACE DIRECTORY '||p_name||' AS '''||p_path||'''';
  end;

  procedure exception_proccessor(
    err_msg in varchar2,
    p_dp_handle in number)
  is
  begin
      dbms_output.put_line('Process exception: Begin');
      dbms_output.put_line(' - Err:'||err_msg);
      DBMS_DATAPUMP.stop_job(
          handle => p_dp_handle,
          immediate => 1,
          keep_master => 0);
      dbms_output.put_line(' - job('||p_dp_handle||'): stopped');
      DBMS_DATAPUMP.detach(p_dp_handle);
      dbms_output.put_line(' - job('||p_dp_handle||'): detached');
      dbms_output.put_line('Process exception: End');
  end;

  procedure run_job(p_dp_handle in number, v_jstate out varchar2)
  is
  begin
    DBMS_DATAPUMP.start_job(p_dp_handle);
    dbms_output.put_line('passed:start_job('||p_dp_handle||')');
    DBMS_DATAPUMP.wait_for_job(p_dp_handle, v_jstate);
    dbms_output.put_line('passed:wait_for_job('||p_dp_handle||':state:'||v_jstate||')');
    DBMS_DATAPUMP.detach(p_dp_handle);
    dbms_output.put_line('passed:detach('||p_dp_handle||')');
  end;

  procedure exp_schema(p_schema_name in varchar2, p_exp_path in varchar2)
  is
    csSCHEMA2BKP varchar2(100):=p_schema_name;
    csDMP_FILE varchar2(100):= gen_dmp_file_name(csSCHEMA2BKP);
    csLOG_FILE varchar2(100):= csDMP_FILE||'(EXP).LOG';
    csExpPath varchar2(500) := p_exp_path;
    csJobName varchar2(100):='JDTPMPEXP_'||csSCHEMA2BKP;
    l_dp_handle number;
    l_jstate varchar2(100);
  begin
    init_ora_directory(csOraDir4ExpImp, csExpPath);
    l_dp_handle := open_dtpump_job(csJobName, 'SCHEMA', null, 'EXPORT');
    dbms_output.put_line('passed:open_dtpump_job('||csJobName||':'||l_dp_handle||')');
    DBMS_DATAPUMP.add_file(    
      handle    => l_dp_handle,
      filename  => csDMP_FILE,
      directory => csOraDir4ExpImp);
    dbms_output.put_line('passed:add_file('||csDMP_FILE||')');
    DBMS_DATAPUMP.add_file(   
      handle    => l_dp_handle,
      filename  => csLOG_FILE,
      directory => csOraDir4ExpImp,
      filetype  => DBMS_DATAPUMP.KU$_FILE_TYPE_LOG_FILE);
    dbms_output.put_line('passed:add_file('||csLOG_FILE||')');
    DBMS_DATAPUMP.metadata_filter(
      handle => l_dp_handle,
      name   => 'SCHEMA_EXPR',
      value  => '= '''||csSCHEMA2BKP||'''');
    dbms_output.put_line('passed:metadata_filter('||csSCHEMA2BKP||')');
    
    run_job(l_dp_handle, l_jstate);
  exception
    when OTHERS then
      exception_proccessor(SQLERRM, l_dp_handle);
  end;

  procedure imp_schema(
    p_from_schema in varchar2, 
    p_from_tss in varchar2,
    p_from_file_name in varchar2, 
    p_from_path in varchar2,
    p_to_schema in varchar2, 
    p_to_tss in varchar2)
  is
    csDMP_FILE varchar2(100):=p_from_file_name;
    csLOG_FILE varchar2(100):= csDMP_FILE||'(IMP).LOG';
    csSCHEMA_FROM varchar2(100):=p_from_schema;
    csSCHEMA_TARGET varchar2(100):=p_to_schema;
    csJobName varchar2(100):='JDTPMPIMP_'||csSCHEMA_FROM;
    csImpPath varchar2(500) := p_from_path;
    l_dp_handle       NUMBER;
    l_jstate varchar2(100);
    tbl_src_tss ai_utl.T_VARCHAR_TBL;
    tbl_trgt_tss ai_utl.T_VARCHAR_TBL;
  begin
    dbms_output.put_line('imp_schema:start(');
    dbms_output.put_line('  p_from_schema:'||p_from_schema);
    dbms_output.put_line('  p_from_tss:'||p_from_tss);
    dbms_output.put_line('  p_from_file_name:'||p_from_file_name);
    dbms_output.put_line('  p_from_path:'||p_from_path);
    dbms_output.put_line('  p_to_schema:'||p_to_schema);
    dbms_output.put_line('  p_to_tss:'||p_to_tss);
    dbms_output.put_line(');');
    select * bulk collect into tbl_src_tss
      from table(ai_utl.split_str(p_from_tss,','));
    select * bulk collect into tbl_trgt_tss
      from table(ai_utl.split_str(p_to_tss,','));
    init_ora_directory(csOraDir4ExpImp, csImpPath);
    dbms_output.put_line('passed:init_ora_directory('||csOraDir4ExpImp||':'||csImpPath||')');
    l_dp_handle := open_dtpump_job(csJobName, 'SCHEMA', null, 'IMPORT');
    dbms_output.put_line('passed:open_dtpump_job('||csJobName||':'||l_dp_handle||')');
    
    DBMS_DATAPUMP.add_file(    
      handle    => l_dp_handle,
      filename  => csDMP_FILE,
      directory => csOraDir4ExpImp);
    dbms_output.put_line('passed:add_file('||csDMP_FILE||')');
  
    DBMS_DATAPUMP.add_file(   
      handle    => l_dp_handle,
      filename  => csLOG_FILE,
      directory => csOraDir4ExpImp,
      filetype  => DBMS_DATAPUMP.KU$_FILE_TYPE_LOG_FILE);dbms_output.put_line('passed:add_file(log)');
    dbms_output.put_line('passed:add_file('||csLOG_FILE||')');
  
    DBMS_DATAPUMP.metadata_filter (
      handle => l_dp_handle ,
      name => 'SCHEMA_LIST' ,
      value => ''''||csSCHEMA_FROM||'''');
    dbms_output.put_line('passed:metadata_filter('||csSCHEMA_FROM||')');
    if(csSCHEMA_TARGET is not null)and(csSCHEMA_TARGET <> csSCHEMA_FROM)then
      DBMS_DATAPUMP.metadata_remap(
        handle => l_dp_handle,
        name => 'REMAP_SCHEMA',
        old_value => csSCHEMA_FROM,
        value => csSCHEMA_TARGET);
      dbms_output.put_line(' - passed:metadata_remap(REMAP_SCHEMA:'||csSCHEMA_FROM||'->'||csSCHEMA_TARGET||')');
      if tbl_trgt_tss.count = tbl_src_tss.count then
        for i in 1..tbl_trgt_tss.count loop
          DBMS_DATAPUMP.metadata_remap(
            handle => l_dp_handle,
            name => 'REMAP_TABLESPACE',
            old_value => tbl_src_tss(i),
            value => tbl_trgt_tss(i));
          dbms_output.put_line(' - passed:metadata_remap(REMAP_TABLESPACE:'||tbl_src_tss(i)||'->'||tbl_trgt_tss(i)||')');
        end loop;
      else
        raise_application_error(-20000, 'Списки переименования TSS должны иметь одинаковое кол-во элементов.');
      end if;
      
    end if;
    DBMS_DATAPUMP.set_parameter (
      handle => l_dp_handle ,
      name => 'TABLE_EXISTS_ACTION' ,
      value => 'REPLACE' );
    dbms_output.put_line('passed:set_parameter(TABLE_EXISTS_ACTION:REPLACE)');
    run_job(l_dp_handle, l_jstate);
  exception
    when OTHERS then
      exception_proccessor(SQLERRM, l_dp_handle);
  end;

end;
/


DROP PACKAGE BODY BIOSYS.AI_ORG;

CREATE OR REPLACE PACKAGE BODY BIOSYS.AI_ORG
IS

  procedure odep_save(
    p_odep_uid in orgstruct.odep_uid%type,
    p_odep_uid_new in out orgstruct.odep_uid%type,
    p_odep_prnt in orgstruct.odep_prnt%type,
    p_odep_name in orgstruct.odep_name%type,
    p_odep_desc in orgstruct.odep_desc%type,
    p_odep_uid_auto in orgstruct.odep_uid_auto%type
    )
  is
    vUID T_GUID := upper(p_odep_uid);
    vUID_NEW T_GUID := upper(p_odep_uid_new);
    vPRNT_UID T_GUID := upper(p_odep_prnt);
    vODEP_EXISTS pls_integer;
    vUidExists  pls_integer;
    v_odep_uid_auto_old pls_integer;
    procedure appendRec(pp_odep_uid in orgstruct.odep_uid%type, 
                        pp_prnt_odep_uid in orgstruct.odep_prnt%type)
    is
    begin
      insert into orgstruct(odep_uid, odep_prnt, odep_name, odep_desc, odep_uid_auto)
        values(upper(pp_odep_uid), decode(upper(pp_prnt_odep_uid), 'ROOT-NODE', null, upper(pp_prnt_odep_uid)), substr(p_odep_name, 1, 500), substr(p_odep_desc, 1, 1000), p_odep_uid_auto);
    end;
    procedure checkOdepUIDExists(pp_odep_uid in orgstruct.odep_uid%type)
    is
      vRslt pls_integer;
    begin
      select count(1) into vRslt
        from orgstruct a
       where a.odep_uid = pp_odep_uid;
      if vRslt > 0 then
        raise_application_error(-20000, 'Подразделение с кодом '||pp_odep_uid||' уже существует.');  
      end if;
    end;
  begin
    --if p_odep_uid is not null then
      select count(1), max(odep_uid_auto) into vODEP_EXISTS, v_odep_uid_auto_old
        from orgstruct a
       where a.odep_uid = vUID;
      v_odep_uid_auto_old:=nvl(v_odep_uid_auto_old, 0);
      /*if (vODEP_EXISTS = 0) and (vPRNT_UID is null) then  
        select max(a.odep_uid) into vPRNT_UID
          from orgstruct a
         where a.org_uid = p_org_uid
           and a.odep_prnt is null;
      end if;*/
      if (vODEP_EXISTS = 0) then
        if (p_odep_uid_auto = 1) then
          vUID := upper(sys_guid());
        else
          if (vUID_NEW is null) then
            vUID := upper(sys_guid());
          else
            vUID := vUID_NEW;
            checkOdepUIDExists(vUID);
          end if;
        end if;
        /*insert into orgstruct(org_uid, odep_uid, odep_prnt, odep_name, odep_desc, odep_uid_auto)
          values(p_org_uid, vUID, vPRNT_UID, p_odep_name, p_odep_desc, p_odep_uid_auto);*/
        dbms_output.put_line('before appendRec. p_odep_name:='||p_odep_name);
        appendRec(vUID, vPRNT_UID);
        vUID_NEW := vUID;
      else
        if ((v_odep_uid_auto_old = 0) and (p_odep_uid_auto = 1)) then
          vUID_NEW := upper(sys_guid());
        end if;
        if (p_odep_uid_auto = 0) then
          if (vUID_NEW is null) then
            vUID_NEW := upper(sys_guid());
          end if;
        end if;
        if vUID_NEW = vUID then
          begin
            update orgstruct a set
              a.odep_uid = vUID_NEW,
              a.odep_name = substr(p_odep_name, 1, 500),
              a.odep_desc = substr(p_odep_desc, 1, 1000),
              a.odep_uid_auto = p_odep_uid_auto
             where a.odep_uid = vUID;
          exception
            when OTHERS then
              raise_application_error(-20000, 'Err: '||sqlerrm||'; vUID:'||vUID||'; vUID_NEW:'||vUID_NEW);
          end;
        else
          checkOdepUIDExists(vUID_NEW);
          appendRec(vUID_NEW, vPRNT_UID);
          update USRS a set a.odep_uid = vUID_NEW
           where a.odep_uid = vUID;
          update orgstruct a set
            a.odep_prnt = vUID_NEW
           where a.odep_prnt = vUID;
          delete from orgstruct a 
           where a.odep_uid = vUID;
        end if;
      end if;
    --end if;
      p_odep_uid_new := vUID_NEW;
  end;

  /*function orgSave(
    p_org_uid in orgs.org_uid%type,
    p_org_name in orgs.org_name%type,
    p_org_desc in orgs.org_desc%type) return T_GUID
  is
    vUID orgs.org_uid%type := p_org_uid;
    vROOT_UID orgstruct.odep_uid%type;
  begin
    if vUID is null then
      select max(a.org_uid) into vUID from orgs a
       where upper(a.org_name) = upper(p_org_name);
      if vUID is not null then
        raise_application_error(-20000, 'Организация с именем "'||p_org_name||'", уже зарегистрирована в системе!');
      end if;
    end if;
    if vUID is null then
      vUID := upper(sys_guid());
      insert into ORGS(ORG_UID, ORG_NAME, ORG_DESC, OWNER_UID)
        values(vUID, p_org_name, p_org_desc, null);
      vROOT_UID := orgDepSave(vUID, null, null, null, 'root', 'root', 1);
    else
      update ORGS a set
        a.org_name = p_org_name,
        a.org_desc = p_org_desc
       where a.org_uid = vUID;
    end if;
    return vUID;
  end;*/

  /*function get_root_odep(
    p_odep_uid in orgstruct.odep_uid%type,
    p_owner_uid in orgstruct.owner_uid%type) return orgstruct.odep_uid%type
  is
    vROOT_UID orgstruct.odep_uid%type;
  begin
    select a.odep_uid into vROOT_UID
      from orgstruct a
      where a.owner_uid = upper(p_owner_uid)
     start with a.odep_uid = upper(p_odep_uid)
    connect by prior  a.odep_prnt = a.odep_uid;
    return vROOT_UID;
  exception
    when NO_DATA_FOUND then
      return null;    
  end;*/

  procedure odep_set_owner(
    p_odep_uid in orgstruct.odep_uid%type,
    p_owner in usrs.usr_uid%type) 
  is
    vROOT_UID orgstruct.odep_uid%type;
  begin
    --vROOT_UID := get_root_odep(p_odep_uid);
    update usrs a set
      a.odep_uid = upper(p_odep_uid)
     where a.usr_uid = upper(p_owner);
    update orgstruct a set
      a.owner_uid = upper(p_owner)
     where a.odep_uid = upper(p_odep_uid);
  end;

/*
  function orgDepSaveEx(
    pa_org_uid in tb_orgs.fa_org_uid%type,
    pa_org_name in tb_orgs.fa_org_name%type,
    pa_org_desc in tb_orgs.fa_org_desc%type,
    pa_odep_prnt in orgstruct.fa_odep_prnt%type,
    pa_odep_uid in orgstruct.odep_uid%type,
    pa_name in orgstruct.fa_name%type,
    pa_desc in orgstruct.fa_desc%type,
    pa_owner in tb_usrs.fa_usr_uid%type) return T_GUID
  is
    vORG_UID tb_orgs.fa_org_uid%type := pa_org_uid;
    vUID orgstruct.odep_uid%type := pa_odep_uid;
  begin
    if vORG_UID is null then
      vORG_UID := orgSave(vORG_UID, pa_org_name, pa_org_desc);
      orgSetOwner(vORG_UID, pa_owner);
    end if;
    vUID := orgDepSave(vORG_UID, pa_odep_prnt, pa_odep_uid, pa_name, pa_desc);
    return vUID;
  end;
*/  
/*  
  procedure orgDelete(pa_org_uid in T_GUID, pa_autor in tb_usrs.fa_usr_uid%type)
  is
  begin
    delete from TB_USRS a
      where exists(select 1 from orgstruct b
                    where a.odep_uid = b.odep_uid
                      and b.fa_org_uid = pa_org_uid);
    delete from orgstruct a
     where a.fa_org_uid = pa_org_uid;
    delete from TB_ORGS a
     where a.fa_org_uid = pa_org_uid;
  end;
*/

  function node_is_child_to(
    parent_node in varchar2,
    child_node in varchar2
    ) return number
  is
    vCheckResul pls_integer;
  begin
    select count(1) --odep_uid, SYS_CONNECT_BY_PATH(fa_name, '/') "Path"
      into vCheckResul
      from orgstruct
     where odep_uid = child_node
     start with odep_prnt = parent_node
    connect by prior odep_uid = odep_prnt;
    return vCheckResul;
  end;
  
  procedure move_obj(
  	P_OBJ_UID in varchar2,
  	P_OBJ_TP  in varchar2,
  	P_ODEP_TO in orgstruct.odep_uid%type)
 	is
 	  vCheck pls_integer;
 	begin
 	  if P_OBJ_TP = '1' then
 	    vCheck := node_is_child_to(P_OBJ_UID, P_ODEP_TO);  
 	    if vCheck > 0 then
 	      raise_application_error(-20000, 'Неозможно перенести отдел в один из его дочерних отделов.');
      end if;
 	    update orgstruct a set
 	      a.odep_prnt = P_ODEP_TO
      where odep_uid = P_OBJ_UID;
 	  elsif P_OBJ_TP = '0' then
 	    update usrs a set 
 	      a.odep_uid = P_ODEP_TO
      where a.usr_uid = P_OBJ_UID;
 	  end if;
 	end;
  	
  /*function getOrgsLoo(pUsrUID in varchar2
    ) return T_ORGLOO_TBL pipelined
  is
    vIsAdmin number(1);
    vIsWsAdmin number(1);
    vRec T_ORGLOO_REC;
    vOrgWS orgs.org_uid%type;
  begin
    ai_admin.detectAdmins(pUsrUID, vIsAdmin, vIsWsAdmin);
    if (vIsAdmin > 0) or (vIsWsAdmin > 0) then
      select max(a.org_uid) into vOrgWS
        from orgs a
       where a.owner_uid = pUsrUID;
       
      for c in (SELECT a.org_uid, a.org_name, a.owner_uid FROM ai_orgs a) loop
        vRec.org_uid := c.org_uid;
        vRec.org_name := c.org_name;
        vRec.owner_uid := c.owner_uid;
        if (vIsAdmin > 0) or 
            ((vIsWsAdmin > 0) and (vRec.org_uid = vOrgWS)) then
          pipe row(vRec);
        end if;
      end loop;
    end if;
  end;*/
  
  /*procedure dropOrg(P_ORG_UID in orgs.org_uid%type)
  is
  begin
    update orgs a set a.owner_uid = null
    where a.org_uid = p_org_uid;
    delete usrs a where a.org_uid = p_org_uid;
    delete orgstruct a where a.org_uid = p_org_uid;
    delete orgs a where a.org_uid = p_org_uid;
  end;*/
  
  procedure odep_drop(
  	P_ODEP_UID in orgstruct.odep_uid%type)
 	is
 	begin
    delete from usrrles a 
     where exists (select 1 from usrs c 
                    where c.usr_uid = a.usr_uid 
                      and c.odep_uid in (select e12.odep_uid 
                                           from orgstruct e12
                                          start with e12.odep_uid = P_ODEP_UID
                                        connect by prior e12.odep_uid = e12.odep_prnt));
    delete from usrs a 
     where a.odep_uid in (select e12.odep_uid 
                            from orgstruct e12
                           start with e12.odep_uid = P_ODEP_UID
                         connect by prior e12.odep_uid = e12.odep_prnt);
    delete from orgstruct a 
     where a.odep_uid in (select e12.odep_uid 
                            from orgstruct e12
                           start with e12.odep_uid = P_ODEP_UID
                         connect by prior e12.odep_uid = e12.odep_prnt);
 	end;
  
end;
/


DROP PACKAGE BODY BIOSYS.AI_SMTP;

CREATE OR REPLACE PACKAGE BODY BIOSYS.ai_smtp
AS
  crlf                      VARCHAR2(2)  := CHR(13) || CHR(10);
  -- A unique string that demarcates boundaries of parts in a multi-part email
  -- The string should not appear inside the body of any part of the email.
  -- Customize this if needed or generate this randomly dynamically.
  boundary_1         CONSTANT VARCHAR2 (256) := 'bnd-1-'||sys_guid()||'';
  boundary_2         CONSTANT VARCHAR2 (256) := 'bnd-2-'||sys_guid()||'';

  cur_boundary_1     CONSTANT VARCHAR2 (256) := '--' || boundary_1;
  last_boundary_1    CONSTANT VARCHAR2 (256) := '--' || boundary_1 || '--';

  cur_boundary_2     CONSTANT VARCHAR2 (256) := '--' || boundary_2;
  last_boundary_2    CONSTANT VARCHAR2 (256) := '--' || boundary_2 || '--';

  procedure wr_data (p_conn       IN OUT NOCOPY UTL_SMTP.connection,
                        p_text       IN VARCHAR2)
  is
  begin
    UTL_SMTP.write_data (p_conn, p_text);
    dbms_output.put_line(p_text);
  end;

  procedure wr_row_data (p_conn       IN OUT NOCOPY UTL_SMTP.connection,
                        p_text       IN VARCHAR2)
  is
  begin
    UTL_SMTP.write_raw_data (p_conn, UTL_RAW.cast_to_raw (p_text));
    dbms_output.put_line(p_text);
  end;

  procedure begin_attachment (
    conn           in out nocopy utl_smtp.connection,
    mime_type      in            varchar2 default 'text/plain',
    filename       in            varchar2 default null,
    transfer_enc   in            varchar2 default null)
  is
  begin
    wr_data (conn, 'Content-Type:' || mime_type || crlf);

    if (filename is not null) then
      wr_row_data (conn, 'Content-Disposition: attachment; filename="' || filename || '"' || crlf);
    end if;

    if (transfer_enc is not null) then
      wr_data (conn, 'Content-Transfer-Encoding: ' || transfer_enc || crlf);
    end if;

    wr_data (conn, crlf);
  end;

  procedure end_attachment (conn   in out nocopy UTL_SMTP.connection)
  is
  begin
    wr_data (conn, crlf);
  end;

  procedure wr_blob (p_conn       in out nocopy UTL_SMTP.connection,
                     p_filename   in         varchar2,
                     p_data       in         blob,
                     p_encoding   in         varchar2)
  is
    max_base64_line_width   constant pls_integer := 76 / 4 * 3;
    l_buffer                         raw (32767);
    l_pos                            pls_integer := 1;
    l_blob_len                       pls_integer;
    l_amount                         pls_integer := 32767;
  begin
    -- Split the Base64-encoded attachment into multiple lines

    -- In writing Base-64 encoded text following the MIME format below,
    -- the MIME format requires that a long piece of data must be splitted
    -- into multiple lines and each line of encoded data cannot exceed
    -- 80 characters, including the new-line characters. Also, when
    -- splitting the original data into pieces, the length of each chunk
    -- of data before encoding must be a multiple of 3, except for the
    -- last chunk. The constant MAX_BASE64_LINE_WIDTH
    -- (76 / 4 * 3 = 57) is the maximum length (in bytes) of each chunk
    -- of data before encoding.
    --

    begin_attachment (p_conn,
                      'application/octet',
                      p_filename,
                      'base64');

    l_blob_len := DBMS_LOB.getlength (p_data);

    --
    --
    --
    --
    while l_pos < l_blob_len
    loop
      l_amount := max_base64_line_width;
      DBMS_LOB.read (p_data,
                     l_amount,
                     l_pos,
                     l_buffer);
      UTL_SMTP.write_raw_data (p_conn, UTL_ENCODE.base64_encode (l_buffer));
      UTL_SMTP.write_data (p_conn, crlf);
      --UTL_SMTP. (conn);
      l_pos := l_pos + max_base64_line_width;
    end loop;

    end_attachment (p_conn);
    dbms_output.put_line('{blob}');
  end;

  procedure wr_body_as_raw (p_conn   in out nocopy UTL_SMTP.connection,
                            p_text   in            clob)
  is
  begin
    if (p_text is not null) then
      wr_row_data (p_conn, crlf || p_text);
      dbms_output.put_line(crlf || '{body}');
    end if;
  end;

  procedure wr_subj_as_raw (p_conn   in out nocopy UTL_SMTP.connection,
                            p_text   in            varchar2)
  is
  begin
    if (p_text is not null) then
      wr_row_data (p_conn, p_text || crlf);
      dbms_output.put_line(p_text || crlf);
    end if;
  end;

  PROCEDURE send (p_mailhost       IN VARCHAR2,
                  p_username       IN VARCHAR2,
                  p_password       IN VARCHAR2,
                  p_port           IN NUMBER,
                  p_sender         IN VARCHAR2,
                  p_recipient      IN VARCHAR2,
                  p_ccrecipient    IN VARCHAR2 default null,
                  p_bccrecipient   IN VARCHAR2 default null,
                  p_subject        IN VARCHAR2 default null,
                  p_message_text   IN CLOB default EMPTY_CLOB(),
                  p_message_html   IN CLOB default EMPTY_CLOB(),
                  p_encoding       IN VARCHAR2 default 'windows-1251',
                  p_filename       IN VARCHAR2 DEFAULT NULL,
                  p_binaryfile     IN BLOB DEFAULT EMPTY_BLOB()
                  )
  IS
    v_conn                         UTL_SMTP.connection;
    v_port                         NUMBER (3) := NVL (p_port, 25);
  BEGIN
    v_conn := UTL_SMTP.open_connection (p_mailhost, v_port);
    UTL_SMTP.ehlo (v_conn, p_mailhost);

    if (p_username is not null) and (p_password is not null)
    then
      UTL_SMTP.command (v_conn, 'AUTH LOGIN');
      UTL_SMTP.command (
        v_conn,
        UTL_RAW.cast_to_varchar2 (
          UTL_ENCODE.base64_encode (UTL_RAW.cast_to_raw (p_username))));
      UTL_SMTP.command (
        v_conn,
        UTL_RAW.cast_to_varchar2 (
          UTL_ENCODE.base64_encode (UTL_RAW.cast_to_raw (p_password))));
    end if;

    UTL_SMTP.helo (v_conn, p_mailhost);
    UTL_SMTP.mail (v_conn, p_sender);
    UTL_SMTP.rcpt (v_conn, p_recipient);


    if p_ccrecipient is not null
    then
      UTL_SMTP.rcpt (v_conn, p_ccrecipient);
    end if;


    if p_bccrecipient is not null
    then
      UTL_SMTP.rcpt (v_conn, p_bccrecipient);
    end if;


    --
    -- Sending data
    --
    UTL_SMTP.open_data (v_conn);
    wr_data (v_conn, 'Date: ' || TO_CHAR (SYSDATE - 0.5, 'dd Mon yy hh24:mi:ss')||crlf);

    wr_data (v_conn, 'To: ' || p_recipient||crlf);
    if(p_ccrecipient is not null)then
      wr_data (v_conn, 'CC: ' || p_ccrecipient||crlf);
    end if;
    wr_data (v_conn, 'From: ' || p_sender||crlf);
    wr_subj_as_raw (v_conn, 'Subject: ' || p_subject);
    wr_data (v_conn, 'MIME-Version: 1.0'||crlf);

    wr_data (v_conn,
      'Content-Type: multipart/mixed; boundary="' || boundary_1 || '"; charset="'||p_encoding||'"'||crlf||crlf);

    wr_data(v_conn, cur_boundary_1||crlf);

    wr_data (v_conn,
      'Content-Type: multipart/alternative; boundary="' || boundary_2 || '"; charset="'||p_encoding||'"'||crlf||crlf);

    if(DBMS_LOB.getlength(p_message_text) > 0)then
      wr_data (v_conn, cur_boundary_2||crlf);
      wr_data (v_conn, 'Content-Type: text/plain; charset="'||p_encoding||'"' || crlf || crlf);
      wr_body_as_raw (v_conn, p_message_text);
      wr_data (v_conn, crlf || crlf);
    end if;

    if(DBMS_LOB.getlength(p_message_html) > 0)then
      wr_data (v_conn, cur_boundary_2||crlf);
      wr_data (v_conn, 'Content-Type: text/html; charset="'||p_encoding||'"' || crlf || crlf);
      wr_body_as_raw (v_conn, p_message_html);
      wr_data (v_conn, crlf || crlf);
    end if;
    wr_data (v_conn, last_boundary_2||crlf);

    if p_filename is not null then
      wr_data (v_conn, cur_boundary_1||crlf);
      --wr_data (v_conn, 'Content-Type: text/plain; charset="'||p_encoding||'"' || crlf || crlf);
      wr_blob (v_conn, p_filename, p_binaryfile, p_encoding);
      wr_data (v_conn, last_boundary_1||crlf);
    else
      wr_data (v_conn, last_boundary_1||crlf);
    end if;

    UTL_SMTP.close_data (v_conn);
    UTL_SMTP.quit (v_conn);
  END;


END;
/


DROP PACKAGE BODY BIOSYS.AI_UTL;

CREATE OR REPLACE PACKAGE BODY BIOSYS.AI_UTL
IS

  /* Вытаскивает первый элемент из списка разделенного разделителями */
  function pop_val_from_list1(
    vLIST in out nocopy CLOB,
    pDelimeter in varchar2 default ',')return CLOB
  is
    rslt CLOB;
    vCommaPos pls_integer;
    vDelimeterLen pls_integer := length(pDelimeter);
  begin
    rslt := null;
    if length(vLIST) > 0  then
        vCommaPos := instr(vLIST, pDelimeter, 1);
        if vCommaPos > 0 then
          rslt := substr(vLIST, 1, vCommaPos-1);
          vLIST := substr(vLIST, vCommaPos+vDelimeterLen);
        else
          rslt := vLIST;
          vLIST := null;
        end if;
    end if;
    return rslt;
  end;

  function pop_val_from_list(
    vLIST in out nocopy varchar2,
    pDelimeter in varchar2 default ',')return varchar2
  is
    rslt varchar2(4000);
    vCommaPos pls_integer;
    vDelimeterLen pls_integer := length(pDelimeter);
  begin
    rslt := null;
    if length(vLIST) > 0  then
        vCommaPos := instr(vLIST, pDelimeter, 1);
        if vCommaPos > 0 then
          rslt := substr(vLIST, 1, vCommaPos-1);
          vLIST := substr(vLIST, vCommaPos+vDelimeterLen);
        else
          rslt := vLIST;
          vLIST := null;
        end if;
    end if;
    return rslt;
  end;

  /* Вытаскивает последний элемент из списка разделенного разделителями */
  function pop_last_val_from_list(
    vLIST in out nocopy VARCHAR2,
    pDelimeter in varchar2 default ',')return VARCHAR2
  is
    rslt VARCHAR2(320);
    vCommaPos integer;
  begin
    rslt := null;
    if length(vLIST) > 0  then
        vCommaPos := instr(vLIST, pDelimeter, -1);
        if vCommaPos > 0 then
          rslt := substr(vLIST, vCommaPos+1);
          vLIST := substr(vLIST, 1, vCommaPos-1);
        else
          rslt := vLIST;
          vLIST := '';
        end if;
    end if;
    return rslt;
  end;

  /* возвращает n-й элемент из строки с разделителями */
  function get_word (pSource varchar2, pIndex number, pDelimeter varchar2 := '/')
      return varchar2
   is
      lStartPosition   binary_integer;
      lEndPosition     binary_integer;
   begin
      if pIndex = 1 then
         lStartPosition := 1;
      else
         lStartPosition := instr (pSource, pDelimeter, 1, pIndex - 1);

         if lStartPosition = 0 then
            return null;
         else
            lStartPosition := lStartPosition + length (pDelimeter);
         end if;
      end if;

      lEndPosition := instr (pSource, pDelimeter, lStartPosition, 1);

      if lEndPosition = 0 then
         return substr (pSource, lStartPosition);
      else
         return substr (pSource, lStartPosition, lEndPosition - lStartPosition);
      end if;
   end;

  /* Проверяет наличие элемента в списке */
  function item_in_list(p_item in varchar2, p_list in varchar2, p_delimeter in varchar2 default ',') return number
  is
    vList varchar2(32760);
    vCurItem varchar2(4000);
    vDelPos integer;
  begin
    vList := p_list;
    while Length(vList) > 0 loop
      vCurItem := pop_val_from_list(vList, p_delimeter);
      if upper(trim(p_item)) = upper(trim(vCurItem)) then
        return 1;
      end if;
    end loop;
    return 0;
  end;

  /* Добавляет элемент в список разделенный разделителями */
  procedure push_val_to_list(
    vLIST in out nocopy varchar2,
    pVal in varchar2,
    pDelimeter in varchar2 default ',')
  is
  begin
    if vLIST is null then
      vLIST:=pVal;
    else
      vLIST:=vLIST||pDelimeter||pVal;
    end if;
  end;

  /* Преобразует список в массив строк */
  function split_str(pStr in varchar2, pDelimeter in varchar2 default ',')
    return T_VARCHAR_TBL pipelined
  is
    vLine varchar2(32000);
    vLine2Add varchar2(1000);
  begin
    vLine:=pStr;
    if instr(vLine, pDelimeter) = 0 then
      if length(vLine) > 0 then
        pipe row (vLine);
      end if;
      vLine:='';
    else
      while length(vLine) > 0 loop
        if instr(vLine, pDelimeter) > 0 then
          vLine2Add:=substr(vLine, 1, instr(vLine, pDelimeter)-1);
          vLine:=substr(vLine, instr(vLine, pDelimeter)+length(pDelimeter));
        else
          vLine2Add:=vLine;
          vLine:='';
        end if;
        pipe row (vLine2Add);
      end loop;
    end if;
  end;

  /* Транспонирование */
  function trans_list(pList in varchar2, pDelimeter in varchar2 default ',')
    return T_INLIST_TBL pipelined
  is
    vList varchar2(32767);
    function newItem(p_value in varchar2) return T_INLIST_REC
    is
      vRslt T_INLIST_REC;
    begin
      vRslt.item := p_value;
      return vRslt;
    end;
  begin
    vList := pList;
    while vList is not null loop
      pipe row(newItem(pop_val_from_list(vList,pDelimeter)));
    end loop;
  end;

  /* Инвертирует список */
  function invert_list(pStr in varchar2, pDelimeter in varchar2 default ',')
    return varchar2
  is
    vLine varchar2(32000);
    vLine2Add varchar2(1000);
    vResult varchar2(32000) := null;
  begin
    vLine:=pStr;
    if instr(vLine, pDelimeter) = 0 then
      if length(vLine) > 0 then
        vResult := vLine;
      end if;
      vLine:='';
    else
      while length(vLine) > 0 loop
        if instr(vLine, pDelimeter) > 0 then
          vLine2Add:=substr(vLine, 1, instr(vLine, pDelimeter)-1);
          vLine:=substr(vLine, instr(vLine, pDelimeter)+1);
        else
          vLine2Add:=vLine;
          vLine:='';
        end if;
        if vResult is null then
          vResult := vLine2Add;
        else
          vResult:= vLine2Add || pDelimeter || vResult;
        end if;
      end loop;
    end if;
    return vResult;
  end;

   /* делает задержку на pInterval секунд */
  function sleep (pInterval in number)
    return number
  is
  begin
     sys.dbms_lock.sleep (pInterval);
     return pInterval;
  end;
  procedure sleep (pInterval in number)
  is
  begin
     sys.dbms_lock.sleep (pInterval);
  end;

  procedure pars_tablename(full_tbl_name in varchar2, schm_name out varchar2, tbl_name out varchar2)
  is
    vParts T_VARCHAR_TBL;
  begin
    select *
      bulk collect into vParts
    from table(split_str(full_tbl_name, '.'));
    if vParts.count = 2 then
      schm_name := vParts(1);
      tbl_name := vParts(2);
    else
      schm_name := null;
      tbl_name := full_tbl_name;
    end if;
  end;

  function lists_is_equals(p_lst1 in varchar2, p_lst2 in varchar2, p_delimeter in varchar2) return number
  is
  begin
    for c in (select column_value from table(split_str(p_lst1, p_delimeter))) loop
      if item_in_list(c.column_value,p_lst2,p_delimeter) = 0 then
        return 0;
      end if;
    end loop;
    for c in (select column_value from table(split_str(p_lst2, p_delimeter))) loop
      if item_in_list(c.column_value,p_lst1,p_delimeter) = 0 then
        return 0;
      end if;
    end loop;
    return 1;
  end;

  function lists_has_common(p_lst1 in varchar2, p_lst2 in varchar2, p_delimeter in varchar2) return number --RESULT_CACHE
  is
    v_list varchar2(32000);
    v_item varchar2(4000);
  begin
    if p_lst1 is null or p_lst2 is null then
      return 0;
    end if;
    v_list := p_lst1;
    while (v_list is not null) loop
      v_item := pop_val_from_list(v_list);
      if item_in_list(v_item,p_lst2) > 0 then
        return 1;
      end if;
    end loop;
    v_list := p_lst2;
    while (v_list is not null) loop
      v_item := pop_val_from_list(v_list);
      if item_in_list(v_item,p_lst1) > 0 then
        return 1;
      end if;
    end loop;
    return 0;
  end;

  procedure compare_lists(
    p_lst1 in varchar2, p_lst2 in varchar2, p_delimeter in varchar2,
    v_add_to_lst1 out varchar2, v_add_to_lst2 out varchar2)
  is
  begin
    for c in (select column_value from table(split_str(p_lst1, p_delimeter))) loop
      if item_in_list(c.column_value,p_lst2,p_delimeter) = 0 then
        push_val_to_list(v_add_to_lst1, c.column_value, p_delimeter);
      end if;
    end loop;
    for c in (select column_value from table(split_str(p_lst2, p_delimeter))) loop
      if item_in_list(c.column_value,p_lst1,p_delimeter) = 0 then
        push_val_to_list(v_add_to_lst2, c.column_value, p_delimeter);
      end if;
    end loop;
  end;

  function pwd (nlength in number)
    return varchar2
  is
    i                              NUMBER        := 1;
    strdoubleconsonants   CONSTANT VARCHAR2 (12) := 'bdfglmnpst';
    strconsonants         CONSTANT VARCHAR2 (20) := 'bcdfghklmnpqrstv';
    strvocal              CONSTANT VARCHAR2 (5)  := 'aeiou';
    generatedpassword              VARCHAR2 (50) := '';
    bmadeconsonant                 BOOLEAN       := FALSE;
    nrnd                           NUMBER;
    nsubrnd                        NUMBER;
    c                              VARCHAR2 (1);
  begin
     for i in 1 .. nlength
     loop
  --get a random number number between 0 and 1
        /*select dbms_random.random
          into nrnd
          from dual;*/
        nrnd := dbms_random.value;

        /*select random.rnd ()
          into nsubrnd
          from dual;*/
        nsubrnd := dbms_random.value;

  /* Simple or double consonant, or a new vocal?
  * Does not start with a double consonant
  * '15% or less chance for the next letter being a double consonant
  */
        IF (generatedpassword <> '') AND (not bmadeconsonant) AND (nrnd < 0.15) THEN
  --double consonant
           c := SUBSTR (strdoubleconsonants, LENGTH (strdoubleconsonants) * nsubrnd + 1, 1);
           c := c || c;
           bmadeconsonant := TRUE;
        ELSIF ((bmadeconsonant <> TRUE) AND (nrnd < 0.95)) THEN
  --80% or less chance for the next letter being a consonant,
  --depending on wether the last letter was a consonant or not.

           --Simple consonant
           c := SUBSTR (strconsonants, LENGTH (strconsonants) * nsubrnd + 1, 1);
           bmadeconsonant := TRUE;
        ELSE
  --5% or more chance for the next letter being a vocal. 100% if last
  --letter was a consonant - theoreticaly speaking...
  --If last one was a consonant, make vocal
           c := SUBSTR (strvocal, LENGTH (strvocal) * nsubrnd + 1, 1);
           bmadeconsonant := FALSE;
        END IF;

        generatedpassword := generatedpassword || c;
     END LOOP;

  --Is the password long enough, or perhaps too long?
     IF (LENGTH (generatedpassword) > nlength) THEN
        generatedpassword := SUBSTR (generatedpassword, 1, nlength);
     END IF;

     RETURN generatedpassword;
  END;
  function encrypt_string(input_string in varchar2, key_line in varchar2) return varchar2
  is
    work_string       varchar2(4000);
    encrypted_string  varchar2(4000);
  begin
    work_string := RPAD
                ( input_string
                , (TRUNC(LENGTH(input_string) / 8) + 1 ) * 8
                , CHR(0));
    DBMS_OBFUSCATION_TOOLKIT.DESENCRYPT (
             input_string     => work_string
           , key_string       => key_line
           , encrypted_string => encrypted_string);
    return encrypted_string;
  end;
  function decrypt_string(input_string in varchar2, key_line in varchar2) return varchar2
  is
    work_string       varchar2(4000);
    decrypted_string  VARCHAR2(4000);
  begin
    DBMS_OBFUSCATION_TOOLKIT.DESDECRYPT
           (
            input_string     => input_string
           ,key_string       => key_line
           ,decrypted_string => work_string
           );

    decrypted_string := RTRIM(work_string, CHR(0));
    return decrypted_string;
--  exception
--    when OTHERS then
--      raise_application_error(-20000, 'Ошибка. Сообщение: '||sqlerrm||'. key_line:['||key_line||']');
  end;

  /* Вычисляет кол-во секунд между заданными датами */
  function time_between_secs(pDtStart in date, pDtEnd in date) return number
  is
    vDaysStart number(32);
    vDaysEnd number(32);
    vSecsStart number(32);
    vSecsEnd number(32);
    vSecsDure number(32);
  begin
    vDaysStart := trunc(pDtStart)-trunc(to_date('19000101', 'YYYYMMDD'));
    vDaysEnd := trunc(pDtEnd)-trunc(to_date('19000101', 'YYYYMMDD'));
    vSecsStart:=24*3600*vDaysStart+to_number(to_char(pDtStart, 'HH24'))*3600+to_number(to_char(pDtStart, 'MI'))*60+to_number(to_char(pDtStart, 'SS'));
    vSecsEnd:=24*3600*vDaysEnd+to_number(to_char(pDtEnd, 'HH24'))*3600+to_number(to_char(pDtEnd, 'MI'))*60+to_number(to_char(pDtEnd, 'SS'));
    vSecsDure:=vSecsEnd-vSecsStart;
    return vSecsDure;
  end;

  /* Вычисляет кол-во дней, часов, мин, секунд из заданного интервала в секундах */
  procedure time_secs_pars(pSecsDure in number,
                        vDayDure out number, vHHDure out number, vMIDure out number, vSSDure out number)
  is
    vSecsRe number(32);
  begin
    vDayDure:=trunc(pSecsDure/(24*3600));
    vSecsRe:=pSecsDure-(vDayDure*(24*3600));
    vHHDure:=trunc(vSecsRe/(3600));
    vSecsRe:=vSecsRe-(vHHDure*(3600));
    vMIDure:=trunc(vSecsRe/(60));
    vSecsRe:=vSecsRe-(vMIDure*(60));
    vSSDure:=vSecsRe;
  end;

  /* Вычисляет кол-во дней, часов, мин, секунд между заданными датами */
  procedure time_between(pDtStart in date, pDtEnd in date,
                        vDayDure out number, vHHDure out number, vMIDure out number, vSSDure out number)
  is
    vSecsDure number(32);
  begin
    vSecsDure:=time_between_secs(pDtStart, pDtEnd);
    time_secs_pars(vSecsDure, vDayDure, vHHDure, vMIDure, vSSDure);
  end;

  /* Вычисляет кол-во дней, часов, мин, секунд в виде строки между заданными датами */
  function time_between_str(pDtStart in date, pDtEnd in date)return varchar2
  is
    vDtStart date := pDtStart;
    vDtEnd date := pDtEnd;
    vDayDure number(3);
    vHHDure number(2);
    vMIDure number(2);
    vSSDure number(2);
    vResult varchar2(64);
    v_over varchar2(10) := null;
  begin
    if (pDtStart is not null) and (pDtEnd is not null) then
      if(vDtStart > vDtEnd)then
        vDtStart := pDtEnd;
        vDtEnd := pDtStart;
        v_over := '+ ';
      end if;
      time_between(vDtStart, vDtEnd, vDayDure, vHHDure, vMIDure, vSSDure);
      vResult:='';
      if vDayDure > 0 then
        vResult:=vResult||to_char(vDayDure)||' дн. ';
      end if;
      vResult:=vResult||trim(to_char(vHHDure, '00'))||':'||trim(to_char(vMIDure, '00'))||':'||trim(to_char(vSSDure, '00'));
      return v_over || vResult;
    else
      return null;
    end if;
  end;

  /* Вычисляет кол-во дней, часов, мин, секунд в виде строки из интервала в секундах */
  function time_secs_str(pSecsDure in number)return varchar2
  is
    vDayDure number(3);
    vHHDure number(2);
    vMIDure number(2);
    vSSDure number(2);
    vResult varchar2(64);
  begin
    time_secs_pars(pSecsDure, vDayDure, vHHDure, vMIDure, vSSDure);
    vResult:='';
    if vDayDure > 0 then
      vResult:=vResult||to_char(vDayDure)||' дн. ';
    end if;
    vResult:=vResult||trim(to_char(vHHDure, '00'))||':'||trim(to_char(vMIDure, '00'))||':'||trim(to_char(vSSDure, '00'));
    return vResult;
  end;

  /* Преобразует период заданный в виде YYYYMM в строку */
  function period_name(pPeriod IN varchar2, pShort in number default 0) return varchar2
  is
    rslt varchar2(128);
    type amnths is varray(12) of varchar2(32);
    mnths amnths := amnths('Январь','Февраль','Март','Апрель','Май','Июнь','Июль','Август','Сентябрь','Октябрь','Ноябрь','Декабрь');
    mnthIndx integer;
  begin
    rslt:='';
    if pPeriod='000000' then
       return 'Не определен';
    end if;
    if length(pPeriod) = 6 then
      mnthIndx := to_number(substr(pPeriod, 5, 2));
      rslt := trim(substr(pPeriod, 1, 4)||' '||mnths(mnthIndx));
    end if;
    if pShort = 1 then
      rslt:=substr(rslt, 1, 8);
    end if;
    return rslt;
    exception
     when others then
      raise_application_error(-20003, ' Ошибка преобразования периода "'||pPeriod||'" в строку. Сообщение: '||SQLERRM);
  end;

  /* Преобразует период заданный в виде YYYYMM в строку */
  function period_name0( pPeriod in varchar2) return varchar2
  is
    rslt varchar2(128);
    type amnths is varray(12) of varchar2(32);
    mnths amnths := amnths('Январь','Февраль','Март','Апрель','Май','Июнь','Июль','Август','Сентябрь','Октябрь','Ноябрь','Декабрь');
    mnthIndx integer;
  begin
    rslt:='';
    if pPeriod='000000' then
       return 'Не определен';
    end if;
    if length(pPeriod) = 6 then
      mnthIndx := to_number(substr(pPeriod, 5, 2));
      rslt := mnths(mnthIndx) || ' ' || trim(substr(pPeriod, 1, 4));
    end if;
    return rslt;
    exception
     when others then
      raise_application_error(-20003, ' Ошибка преобразования периода "'||pPeriod||'" в строку. Сообщение: '||SQLERRM);
  end;

  FUNCTION num_to_word(vpr_num IN NUMBER) RETURN VARCHAR2 IS
  --Функция по переводу числа (денег) в число прописью по типу
  --123.30 -> сто двадцать три рубля 30 копеек
    TYPE t_numarr IS VARRAY(3) OF VARCHAR2(3);
    v_numarr t_numarr;
    v_number PLS_INTEGER := TRUNC(vpr_num); --123 456 789;
    v_kops PLS_INTEGER := (vpr_num - v_number) * 100;
    v_num_str VARCHAR2(9) := TO_CHAR(v_number);
    v_dgroup PLS_INTEGER;
    v_string VARCHAR2(200);
    v_tmp_str VARCHAR2(50);
    v_pos PLS_INTEGER;
    FUNCTION get_dgword(p_num IN VARCHAR2, p_mltply IN PLS_INTEGER) RETURN VARCHAR2 IS
    BEGIN
      RETURN CASE WHEN p_num = '1' THEN
                    CASE WHEN p_mltply IN (1, 7) THEN 'один '
                         WHEN p_mltply IN (3, 6, 9) THEN 'сто '
                         WHEN p_mltply IN (4) THEN 'одна ' END
                  WHEN p_num = '2' THEN
                    CASE WHEN p_mltply IN (1, 7) THEN 'два '
                         WHEN p_mltply IN (2, 5, 8) THEN 'двадцать '
                         WHEN p_mltply IN (3, 6, 9) THEN 'двести '
                         WHEN p_mltply IN (4) THEN 'две ' END
                  WHEN p_num = '3' THEN
                    CASE WHEN p_mltply IN (1, 4, 7) THEN 'три '
                         WHEN p_mltply IN (2, 5, 8) THEN 'тридцать '
                         WHEN p_mltply IN (3, 6, 9) THEN 'триста ' END
                  WHEN p_num = '4' THEN
                    CASE WHEN p_mltply IN (1, 4, 7) THEN 'четыре '
                         WHEN p_mltply IN (2, 5, 8) THEN 'сорок '
                         WHEN p_mltply IN (3, 6, 9) THEN 'четыреста ' END
                  WHEN p_num = '5' THEN
                    CASE WHEN p_mltply IN (1, 4, 7) THEN 'пять '
                         WHEN p_mltply IN (2, 5, 8) THEN 'пятьдесят '
                         WHEN p_mltply IN (3, 6, 9) THEN 'пятьсот ' END
                  WHEN p_num = '6' THEN
                    CASE WHEN p_mltply IN (1, 4, 7) THEN 'шесть '
                         WHEN p_mltply IN (2, 5, 8) THEN 'шестьдесят '
                         WHEN p_mltply IN (3, 6, 9) THEN 'шестьсот ' END
                  WHEN p_num = '7' THEN
                    CASE WHEN p_mltply IN (1, 4, 7) THEN 'семь '
                         WHEN p_mltply IN (2, 5, 8) THEN 'семьдесят '
                         WHEN p_mltply IN (3, 6, 9) THEN 'семьсот ' END
                  WHEN p_num = '8' THEN
                    CASE WHEN p_mltply IN (1, 4, 7) THEN 'восемь '
                         WHEN p_mltply IN (2, 5, 8) THEN 'восемьдесят '
                         WHEN p_mltply IN (3, 6, 9) THEN 'восемьсот ' END
                  WHEN p_num = '9' THEN
                    CASE WHEN p_mltply IN (1, 4, 7) THEN 'девять '
                         WHEN p_mltply IN (2, 5, 8) THEN 'девяносто '
                         WHEN p_mltply IN (3, 6, 9) THEN 'девятьсот ' END
             END;
    END;
    FUNCTION get_tenths(p_num IN VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
      RETURN CASE WHEN p_num = '10' THEN 'десять '
                  WHEN p_num = '11' THEN 'одиннадцать '
                  WHEN p_num = '12' THEN 'двенадцать '
                  WHEN p_num = '13' THEN 'тринадцать '
                  WHEN p_num = '14' THEN 'четырнадцать '
                  WHEN p_num = '15' THEN 'пятнадцать '
                  WHEN p_num = '16' THEN 'шестнадцать '
                  WHEN p_num = '17' THEN 'семнадцать '
                  WHEN p_num = '18' THEN 'восемнадцать '
                  WHEN p_num = '19' THEN 'девятнадцать '
             END;
    END;
    FUNCTION get_names(p_num IN VARCHAR2, p_mltply IN PLS_INTEGER) RETURN VARCHAR2 IS
    BEGIN
      RETURN CASE WHEN p_mltply = 0 THEN
                    CASE WHEN p_num IN ('1') THEN 'копейка '
                         WHEN p_num IN ('2', '3', '4') THEN 'копейки '
                         ELSE 'копеек '
                    END
                  WHEN p_mltply = 1 THEN
                    CASE WHEN p_num IN ('1') THEN 'рубль '
                         WHEN p_num IN ('2', '3', '4') THEN 'рубля '
                         ELSE 'рублей '
                    END
                  WHEN p_mltply = 4 THEN
                    CASE WHEN p_num IN ('1') THEN 'тысяча '
                         WHEN p_num IN ('2', '3', '4') THEN 'тысячи '
                         ELSE 'тысяч '
                    END
                  WHEN p_mltply = 7 THEN
                    CASE WHEN p_num IN ('1') THEN 'миллион '
                         WHEN p_num IN ('2', '3', '4') THEN 'миллиона '
                         ELSE 'миллионов '
                    END
             END;
    END;
  BEGIN
    v_numarr := t_numarr();

    WHILE v_number >= 1 LOOP
      v_dgroup := MOD(v_number, 1000);
      v_number := TRUNC(v_number / 1000);
      v_numarr.EXTEND;
      v_numarr(v_numarr.LAST) := TO_CHAR(v_dgroup);
    END LOOP;
    FOR idx IN 1..v_numarr.COUNT LOOP
      IF SUBSTR(v_numarr(idx), -2, 1) = '1' THEN
        v_tmp_str := get_tenths(SUBSTR(v_numarr(idx), -2, 2))||
                     get_names(SUBSTR(v_numarr(idx), -2, 2), (idx - 1) * 3 + 1);
        v_string := get_dgword(SUBSTR(v_numarr(idx), -3, 1), (idx - 1) * 3 + 3)||v_tmp_str||v_string;
      ELSE
        v_pos := 1;
        FOR i IN REVERSE 1..LENGTH(v_numarr(idx)) LOOP
          v_tmp_str := get_dgword(SUBSTR(v_numarr(idx), i, 1), (idx - 1) * 3 + v_pos)||
                       get_names(SUBSTR(v_numarr(idx), i, 1), (idx - 1) * 3 + v_pos);
          v_string := v_tmp_str||v_string;
          v_pos := v_pos + 1;
        END LOOP;
      END IF;
    END LOOP;
    v_tmp_str := TRIM(TO_CHAR(v_kops, '00'));
    IF SUBSTR(v_tmp_str, -2, 1) = '1' THEN
      v_tmp_str := v_tmp_str||' '||get_names(0, 0);
    ELSE
      v_tmp_str := v_tmp_str||' '||get_names(SUBSTR(v_tmp_str, -1, 1), 0);
    END IF;
    v_string := v_string||v_tmp_str;
    RETURN TRIM(v_string);
  END;

  function vchar2num(pAttrValue in varchar2) return number
  is
  begin
    return TO_NUMBER(replace(pAttrValue, ',', '.'), '99999999.9999999');
  end;

  /* Добавляет месяцы в период, заданный в виде YYYYMM */
  FUNCTION add_periods(pPeriod IN VARCHAR2, pMonths IN PLS_INTEGER) RETURN VARCHAR2 IS
  BEGIN
    return to_char(add_months(to_date(pPeriod, 'YYYYMM'), pMonths), 'YYYYMM');
  END;

  /* Добавляет дни к дате */
  FUNCTION add_days(pDate IN DATE, pDays IN PLS_INTEGER) RETURN DATE IS
  BEGIN
    return pDate + pDays;
  END;

  /* Добавляет 1 месяц в период, заданный в виде YYYYMM */
  function next_period(pPeriod in varchar2) return varchar2 is
  BEGIN
    return add_periods(pPeriod, 1);
  end;

  /* Вычитает 1 месяц в период, заданный в виде YYYYMM */
  function prev_period(pPeriod in varchar2) return varchar2 is
  begin
    return add_periods(pPeriod, -1);
  end;

  /* Вычисляет кол-во полных лет относительно текущей даты */
  function age_calc(borndate in date) return  number
  is
    years number(5);
    months number(5);
  begin
    years:=TO_NUMBER(TO_CHAR(sysdate,'yyyy'))-TO_NUMBER(TO_CHAR(borndate,'yyyy'));
    months:=TO_NUMBER(TO_CHAR(sysdate,'mm'))-TO_NUMBER(TO_CHAR(borndate,'mm'));
    return round(years+months/12);
  end;

  function bitor( x in number, y in number ) return number  as
  begin
    return x + y - bitand(x,y);
  end;

  function bitxor( x in number, y in number ) return number
  is
  begin
    return bitor(x,y) - bitand(x,y);
  end;

  function pars_time(pTime in varchar2) return number
  is
  begin
    return to_date(pTime, 'HH24:MI:SS') - to_date('00:00:00', 'HH24:MI:SS');
  end;
  procedure pars_login(p_login in varchar2, v_usr_name out varchar2, v_usr_pwd out varchar2)
  is
    vResult T_VARCHAR_TBL;
  begin
    v_usr_name := null;
    v_usr_pwd := null;
    select *
      bulk collect into vResult
      from table(ai_utl.split_str(PSTR=>p_login, PDELIMETER=>'/'));
    if(vResult.count = 2)then
      v_usr_name := vResult(1);
      v_usr_pwd := vResult(2);
    end if;
  end;

  function extract_day(p_date date) return number
  is
    v_day varchar2(100) := TRIM(TO_CHAR(p_date, 'DAY'));
    v_result number(1);
  begin
    v_result := case
                  when v_day = 'MONDAY' then 1
                  when v_day = 'TUESDAY' then 2
                  when v_day = 'WEDNESDAY' then 3
                  when v_day = 'THURSDAY' then 4
                  when v_day = 'FRIDAY' then 5
                  when v_day = 'SATURDAY' then 6
                  when v_day = 'SUNDAY' then 7
                end;
    return v_result;
  end;
END;
/


DROP PACKAGE BODY BIOSYS.BIO_LOGIN;

CREATE OR REPLACE PACKAGE BODY BIOSYS.bio_login
IS

  bad_login  EXCEPTION;
  PRAGMA EXCEPTION_INIT(bad_login, -20401);
  user_blocked  EXCEPTION;
  PRAGMA EXCEPTION_INIT(user_blocked, -20402);
  user_not_confirmed  EXCEPTION;
  PRAGMA EXCEPTION_INIT(user_not_confirmed, -20403);
  user_deleted  EXCEPTION;
  PRAGMA EXCEPTION_INIT(user_deleted, -20404);


  cs_context_name varchar2(100) := 'BIO_CONTEXT';
  cs_KeyLine varchar2(100) := 'sinoptic';

  procedure reg_raddr(
    p_session_remote_ip in RADDRSS.rem_addr%type,
    p_session_remote_host in RADDRSS.rem_host%type
  )
  is
    vExists pls_integer;
  begin
    select count(1) into vExists
      from ai_raddrss a
     where a.rem_addr = p_session_remote_ip;
    if(vExists = 0) then
      insert into RADDRSS(rem_addr,rem_host,addr_desc)
      values(substr(p_session_remote_ip, 1, 32), substr(p_session_remote_host, 1, 100), null);
    end if;
  end;

  procedure kill_old_usr(p_usr in varchar2)
  is
  begin
    update usrs a set a.garbaged = 1
     where ((a.usr_name = p_usr) or (a.usr_uid = upper(p_usr)))
       and (((a.reg_date + 1) < sysdate) and (a.confirmed = 0));
  end;

  procedure reg_connection(
    p_user in t_user_name,
    p_session_id in t_session_id,
    p_session_remote_ip in t_ip,
    p_session_remote_host in t_host,
    p_session_remote_client in t_client,
    p_status in t_status
  )
  is
    vNextNumOfIn USRIN$LOG.usrin_num%type;
    vUser USRIN$LOG.usr_name%type := upper(p_user);
    vCurSessionID USRIN$LOG.session_id%type := upper(substr(p_session_id, 1, 32));
  begin
    reg_RAddr(p_Session_Remote_IP, p_Session_Remote_Host);
    select seq_usrin_log.NEXTVAL into vNextNumOfIn from dual;
    insert into USRIN$LOG(rem_addr,usrin_num,usr_name,session_id,rem_client,astatus,usrin_date)
    values(substr(p_Session_Remote_IP, 1, 32), vNextNumOfIn, substr(nvl(vUser, '<noname>'), 1, 64), vCurSessionID, substr(nvl(p_Session_Remote_Client, '<не''что>'), 1, 1000), substr(p_Status, 1, 200), sysdate);
    kill_old_usr(vUser);
  end;

  function get_usr(p_login in t_login) return usr_tbl pipelined
  --function get_usr_list(p_usr_uid in varchar2) return t_usr_tbl pipelined
  is
    v_usr_name varchar2(32) := null;
    v_usr_pwd varchar2(32) := null;
    vRslt usr_rec;
    vRec ai_usrs_edlist%ROWTYPE;
    type t_cur is ref cursor return ai_usrs_edlist%ROWTYPE;
    cur t_cur;
  begin
    ai_utl.pars_login(p_login, v_usr_name, v_usr_pwd);
    open cur for
      SELECT a.* FROM ai_usrs_edlist a
       WHERE a.usr_name = upper(v_usr_name)
         AND ai_utl.decrypt_string(a.usr_pwd,cs_KeyLine) = v_usr_pwd;

    fetch cur into vRec;
    if not cur%notfound then

      if(vRec.confirmed <> 1)then
        raise user_not_confirmed;
      end if;
      if(vRec.blocked = 1)then
        raise user_blocked;
      end if;
      if(vRec.garbaged = 1)then
        raise user_deleted;
      end if;

      vRslt.usr_uid       := vRec.usr_uid;
      vRslt.usr_name      := vRec.usr_name;
      vRslt.usr_fio_fam   := vRec.usr_fam;
      vRslt.usr_fio_name  := vRec.usr_fname;
      vRslt.usr_fio_sname := vRec.usr_sname;
      vRslt.reg_date      := vRec.reg_date;
      vRslt.email_addr    := vRec.email_addr;
      vRslt.usr_phone     := vRec.usr_phone;
      vRslt.odep_uid      := vRec.odep_uid;
      vRslt.odep_path     := vRec.odep_path;
      vRslt.odep_uid_path := vRec.odep_uid_path;
      vRslt.odep_name     := vRec.odep_name;
      vRslt.odep_desc     := vRec.odep_desc;
      vRslt.extinfo       := vRec.extinfo;

      vRslt.usr_roles:=ai_admin.get_usr_roles(vRslt.usr_uid);
      vRslt.owner_uid:=ai_admin.get_odep_owner(vRslt.odep_uid);
      ai_admin.detect_admins(vRslt.usr_uid, vRslt.is_admin, vRslt.is_wsadmin);
      pipe row(vRslt);
    else
      raise bad_login;
    end if;
    close cur;
  end;

  procedure set_context_value(p_name in varchar2, p_value in varchar2)
  is
  begin
    dbms_session.set_context(csBIO_CONTEXT, p_name, p_value);
  end;

  function get_context_value(p_name in varchar2) return varchar2-- RESULT_CACHE
  is
  begin
    return sys_context(csBIO_CONTEXT, p_name);
  end;

END;
/


DROP PACKAGE BODY BIOSYS.DDL$UTL;

CREATE OR REPLACE PACKAGE BODY BIOSYS.DDL$UTL
IS

  /* tbl_name передается в виде <имя схемы>.<имя таблицы> */
  procedure reset_seq(seq_name in varchar2, tbl_name in varchar2, id_fld_name in varchar2)
  is
    vSQL varchar2(1024);
    vMax integer;
    v_seq_name varchar2(32) := seq_name;
    v_schema_name varchar2(32);
    v_table_name varchar2(32);
  begin
    ai_utl.pars_tablename(tbl_name,v_schema_name,v_table_name);
    if v_schema_name is not null then
      v_seq_name := v_schema_name||'.'||v_seq_name;
    end if;

    vSQL := 'select max('||id_fld_name||') from '||tbl_name;
    execute immediate vSQL into vMax;
    vMax := nvl(vMax, 0) + 1;
    vSQL := 'DROP SEQUENCE '||v_seq_name;
    begin
      execute immediate vSQL;
    exception
      when OTHERS then
        vSQL := '';
    end;
    vSQL := 'CREATE SEQUENCE '||v_seq_name||' INCREMENT BY 1 START WITH '||vMax||' MINVALUE 0 MAXVALUE 999999999 CYCLE NOORDER CACHE 2';
    execute immediate vSQL;
  end;

END;
/


GRANT EXECUTE ON BIOSYS.AI_ADMIN TO GIVCADMIN;

GRANT EXECUTE ON BIOSYS.AI_SMTP TO GIVCADMIN;

GRANT EXECUTE ON BIOSYS.DDL$UTL TO GIVCADMIN;

GRANT EXECUTE ON BIOSYS.AI_UTL TO GIVC_ORG;

GRANT EXECUTE ON BIOSYS.AI_ORG TO PUBLIC;

GRANT EXECUTE ON BIOSYS.AI_UTL TO PUBLIC;

GRANT EXECUTE ON BIOSYS.BIO_LOGIN TO PUBLIC;
DROP VIEW BIOSYS.AI_DBS;

/* Formatted on 27.06.2012 22:33:47 (QP5 v5.163.1008.3004) */
CREATE OR REPLACE FORCE VIEW BIOSYS.AI_DBS
(
   CON_UID,
   CON_STR,
   IS_DEFAULT
)
AS
   SELECT a.con_uid, a.con_str, a.is_default
     FROM dbcons a;


DROP VIEW BIOSYS.AI_ORGSTRUCT;

/* Formatted on 27.06.2012 22:33:47 (QP5 v5.163.1008.3004) */
CREATE OR REPLACE FORCE VIEW BIOSYS.AI_ORGSTRUCT
(
   ODEP_UID,
   ODEP_PRNT,
   ODEP_PRNT_CONN,
   ODEP_NAME,
   ODEP_DESC,
   ODEP_UID_AUTO,
   OWNER_UID
)
AS
   SELECT a.odep_uid,
          NVL (a.odep_prnt, 'ROOT-NODE') AS odep_prnt,
          a.odep_prnt AS odep_prnt_conn,
          a.odep_name,
          a.odep_desc,
          a.odep_uid_auto,
          ai_admin.get_odep_owner (a.odep_uid) AS owner_uid
     FROM orgstruct a;


DROP VIEW BIOSYS.AI_ORGSTRUCT_PATH;

/* Formatted on 27.06.2012 22:33:47 (QP5 v5.163.1008.3004) */
CREATE OR REPLACE FORCE VIEW BIOSYS.AI_ORGSTRUCT_PATH
(
   ODEP_UID,
   ODEP_PRNT,
   ODEP_NAME,
   ODEP_DESC,
   F_PATH,
   F_UID_PATH
)
AS
   SELECT ODEP_UID,
          ODEP_PRNT,
          ODEP_NAME,
          ODEP_DESC,
          REPLACE (F_PATH, '|-|', '/') AS F_PATH,
          REPLACE (F_UID_PATH, '|-|', '/') AS F_UID_PATH
     FROM (    SELECT a.odep_uid,
                      NVL (a.odep_prnt, 'ROOT-NODE') AS odep_prnt,
                      a.odep_name,
                      a.odep_desc,
                      SYS_CONNECT_BY_PATH (a.odep_name, '|-|') AS f_path,
                      SYS_CONNECT_BY_PATH (a.odep_uid, '|-|') AS f_uid_path
                 FROM orgstruct a
           START WITH a.odep_prnt IS NULL
           CONNECT BY PRIOR a.odep_uid = a.odep_prnt);


DROP VIEW BIOSYS.AI_UROLES;

/* Formatted on 27.06.2012 22:33:47 (QP5 v5.163.1008.3004) */
CREATE OR REPLACE FORCE VIEW BIOSYS.AI_UROLES
(
   ROLE_UID,
   ROLE_NAME,
   ROLE_IS_SYS,
   ROLE_DESC
)
AS
   SELECT a.role_uid,
          a.role_name,
          a.role_is_sys,
          a.role_desc
     FROM uroles a;


DROP VIEW BIOSYS.AI_USRIN_LOG;

/* Formatted on 27.06.2012 22:33:47 (QP5 v5.163.1008.3004) */
CREATE OR REPLACE FORCE VIEW BIOSYS.AI_USRIN_LOG
(
   REM_ADDR,
   USRIN_NUM,
   USR_NAME,
   SESSION_ID,
   REM_CLIENT,
   ASTATUS,
   USRIN_DATE
)
AS
     SELECT a.rem_addr,
            a.usrin_num,
            a.usr_name,
            a.session_id,
            a.rem_client,
            a.astatus,
            a.usrin_date
       FROM usrin$log a
   ORDER BY a.usrin_date DESC;


DROP VIEW BIOSYS.AI_USRS;

/* Formatted on 27.06.2012 22:33:47 (QP5 v5.163.1008.3004) */
CREATE OR REPLACE FORCE VIEW BIOSYS.AI_USRS
(
   USR_UID,
   ODEP_UID,
   ODEP_PATH,
   CON_UID,
   USR_NAME,
   USR_PWD,
   USR_FAM,
   USR_FNAME,
   USR_SNAME,
   USR_FIO,
   USR_FIO_SHRT,
   REG_DATE,
   BLOCKED,
   EMAIL_ADDR,
   USR_PHONE,
   CONFIRMED,
   GARBAGED,
   EXTINFO
)
AS
   SELECT a.usr_uid,
          a.odep_uid, /*(select max(o.f_path) from ai_orgstruct_path o where a.odep_uid = o.odep_uid)*/
          o.f_path AS odep_path,
          a.con_uid,
          a.usr_name,
          a.usr_pwd,
          INITCAP (TRIM (a.usr_fam)) AS usr_fam,
          INITCAP (TRIM (a.usr_fname)) AS usr_fname,
          INITCAP (TRIM (a.usr_sname)) AS usr_sname,
             INITCAP (TRIM (a.usr_fam))
          || ' '
          || INITCAP (TRIM (a.usr_fname))
          || ' '
          || INITCAP (TRIM (a.usr_sname))
             AS usr_fio,
             INITCAP (TRIM (a.usr_fam))
          || ' '
          || SUBSTR (TRIM (a.usr_fname), 1, 1)
          || '. '
          || SUBSTR (UPPER (a.usr_sname), 1, 1)
          || '.'
             AS usr_fio_shrt,
          a.reg_date,
          a.blocked,
          a.email_addr,
          a.usr_phone,
          a.confirmed,
          a.garbaged,
          a.extinfo
     FROM usrs a, ai_orgstruct_path o
    WHERE a.odep_uid = o.odep_uid;


DROP VIEW BIOSYS.AI_USRS_EDLIST;

/* Formatted on 27.06.2012 22:33:47 (QP5 v5.163.1008.3004) */
CREATE OR REPLACE FORCE VIEW BIOSYS.AI_USRS_EDLIST
(
   USR_UID,
   CON_UID,
   CON_STR,
   USR_NAME,
   USR_PWD,
   USR_FAM,
   USR_FNAME,
   USR_SNAME,
   REG_DATE,
   BLOCKED,
   BLOCKED_CPTN,
   USR_ROLES,
   EMAIL_ADDR,
   USR_PHONE,
   ODEP_UID,
   ODEP_PATH,
   ODEP_UID_PATH,
   ODEP_NAME,
   ODEP_DESC,
   CONFIRMED,
   CONFIRMED_CPTN,
   OWNER_UID,
   GARBAGED,
   IS_ADMIN,
   IS_WSADMIN,
   EXTINFO
)
AS
   SELECT a.usr_uid,
          a.con_uid,
          d.con_str,
          a.usr_name,
          a.usr_pwd,
          a.usr_fam,
          a.usr_fname,
          a.usr_sname,
          a.reg_date,
          a.blocked,
          DECODE (a.blocked, 1, 'да', '') AS blocked_cptn,
          NULL AS usr_roles,
          a.email_addr,
          a.usr_phone,
          a.odep_uid,
          LTRIM (o.f_path, '/') AS odep_path,
          LTRIM (o.f_uid_path, '/') AS odep_uid_path,
          o.odep_name,
          o.odep_desc,
          a.confirmed,
          DECODE (a.confirmed, 1, 'активирован', '')
             AS confirmed_cptn,
          NULL AS owner_uid,
          a.garbaged,
          0 AS is_admin,
          0 AS is_wsadmin,
          a.extinfo
     FROM ai_usrs a, ai_orgstruct_path o, dbcons d
    WHERE a.odep_uid = o.odep_uid AND d.con_uid = a.con_uid;


DROP VIEW BIOSYS.AI_RADDRSS;

/* Formatted on 27.06.2012 22:33:47 (QP5 v5.163.1008.3004) */
CREATE OR REPLACE FORCE VIEW BIOSYS.AI_RADDRSS
(
   REM_ADDR,
   REM_HOST,
   ADDR_DESC,
   LASTIN_DATE
)
AS
   SELECT '*' AS rem_addr,
          'все адреса' AS rem_host,
          '' AS addr_desc,
          NULL AS lastin_date
     FROM DUAL
   UNION
   SELECT a.rem_addr,
          a.rem_host,
          a.addr_desc,
          (SELECT MAX (b.usrin_date)
             FROM AI_USRIN_LOG b
            WHERE b.rem_addr = a.rem_addr)
             AS lastin_date
     FROM raddrss a;


GRANT SELECT ON BIOSYS.AI_ORGSTRUCT_PATH TO PUBLIC;

GRANT SELECT ON BIOSYS.AI_USRIN_LOG TO PUBLIC;

GRANT SELECT ON BIOSYS.AI_RADDRSS TO PUBLIC;
ALTER TABLE BIOSYS.DBCONS
 DROP PRIMARY KEY CASCADE;

DROP TABLE BIOSYS.DBCONS CASCADE CONSTRAINTS;

CREATE TABLE BIOSYS.DBCONS
(
  CON_UID     VARCHAR2(32 BYTE)                 NOT NULL,
  CON_STR     VARCHAR2(500 BYTE)                NOT NULL,
  IS_DEFAULT  NUMBER(1)                         DEFAULT 0                     NOT NULL
)
TABLESPACE BIOSYS_DT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOLOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE BIOSYS.DBCONS IS 'Соединения с БД';

COMMENT ON COLUMN BIOSYS.DBCONS.CON_UID IS 'UID соединения';

COMMENT ON COLUMN BIOSYS.DBCONS.CON_STR IS 'Строка соединения';

COMMENT ON COLUMN BIOSYS.DBCONS.IS_DEFAULT IS 'Соединение используемое по умолчанию';



ALTER TABLE BIOSYS.RADDRSS
 DROP PRIMARY KEY CASCADE;

DROP TABLE BIOSYS.RADDRSS CASCADE CONSTRAINTS;

CREATE TABLE BIOSYS.RADDRSS
(
  REM_ADDR   VARCHAR2(32 BYTE)                  NOT NULL,
  REM_HOST   VARCHAR2(100 BYTE)                 NOT NULL,
  ADDR_DESC  VARCHAR2(1000 BYTE)
)
TABLESPACE BIOSYS_DT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOLOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE BIOSYS.RADDRSS IS 'Справочник адресов';

COMMENT ON COLUMN BIOSYS.RADDRSS.REM_ADDR IS 'Адрес пользователя';

COMMENT ON COLUMN BIOSYS.RADDRSS.REM_HOST IS 'Имя хоста пользователя';

COMMENT ON COLUMN BIOSYS.RADDRSS.ADDR_DESC IS 'Описание';



ALTER TABLE BIOSYS.SMTP$LOG
 DROP PRIMARY KEY CASCADE;

DROP TABLE BIOSYS.SMTP$LOG CASCADE CONSTRAINTS;

CREATE TABLE BIOSYS.SMTP$LOG
(
  ERR_UID     VARCHAR2(32 BYTE)                 NOT NULL,
  CRE_DATE    DATE                              NOT NULL,
  SRV_ADDR    VARCHAR2(100 BYTE)                NOT NULL,
  PORT_NUM    NUMBER(6)                         NOT NULL,
  USR_NAME    VARCHAR2(64 BYTE),
  USR_PWD     VARCHAR2(32 BYTE),
  ADDR_FROM   VARCHAR2(100 BYTE)                NOT NULL,
  ADDR_TO     VARCHAR2(100 BYTE)                NOT NULL,
  MSG_SUBJ    VARCHAR2(1000 BYTE),
  ERROR_TEXT  CLOB                              NOT NULL
)
LOB (ERROR_TEXT) STORE AS (
  TABLESPACE BIOSYS_DT
  ENABLE       STORAGE IN ROW
  CHUNK       8192
  RETENTION
  NOCACHE
  NOLOGGING
  INDEX       (
        TABLESPACE BIOSYS_DT
        STORAGE    (
                    INITIAL          64K
                    NEXT             1M
                    MINEXTENTS       1
                    MAXEXTENTS       UNLIMITED
                    PCTINCREASE      0
                    BUFFER_POOL      DEFAULT
                   ))
      STORAGE    (
                  INITIAL          64K
                  NEXT             1M
                  MINEXTENTS       1
                  MAXEXTENTS       UNLIMITED
                  PCTINCREASE      0
                  BUFFER_POOL      DEFAULT
                 ))
TABLESPACE BIOSYS_DT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOLOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE BIOSYS.SMTP$LOG IS 'Лог ошибок отправки сообщений по SMTP';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.ERR_UID IS 'UID ошибки';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.CRE_DATE IS 'Дата/время';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.SRV_ADDR IS 'Сервер';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.PORT_NUM IS 'Порт';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.USR_NAME IS 'Пользователь';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.USR_PWD IS 'Пароль';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.ADDR_FROM IS 'Адрес отправителя';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.ADDR_TO IS 'Адрес получателя';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.MSG_SUBJ IS 'Тема сообщения';

COMMENT ON COLUMN BIOSYS.SMTP$LOG.ERROR_TEXT IS 'Текст ошибки';



ALTER TABLE BIOSYS.UROLES
 DROP PRIMARY KEY CASCADE;

DROP TABLE BIOSYS.UROLES CASCADE CONSTRAINTS;

CREATE TABLE BIOSYS.UROLES
(
  ROLE_UID     VARCHAR2(64 BYTE)                NOT NULL,
  ROLE_NAME    VARCHAR2(200 BYTE)               NOT NULL,
  ROLE_IS_SYS  NUMBER(1)                        DEFAULT 0                     NOT NULL,
  ROLE_DESC    VARCHAR2(1000 BYTE)
)
TABLESPACE BIOSYS_DT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOLOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE BIOSYS.UROLES IS 'Роли';

COMMENT ON COLUMN BIOSYS.UROLES.ROLE_UID IS 'UID роли';

COMMENT ON COLUMN BIOSYS.UROLES.ROLE_NAME IS 'Имя роли';

COMMENT ON COLUMN BIOSYS.UROLES.ROLE_IS_SYS IS 'Системная роль - Данную роль нельзя добавит или убрать в ручную, она сама добаваляется/убирается в зависимости от других атрибутов. См описание роли.';

COMMENT ON COLUMN BIOSYS.UROLES.ROLE_DESC IS 'Описание роли';



ALTER TABLE BIOSYS.USRIN$LOG
 DROP PRIMARY KEY CASCADE;

DROP TABLE BIOSYS.USRIN$LOG CASCADE CONSTRAINTS;

CREATE TABLE BIOSYS.USRIN$LOG
(
  REM_ADDR    VARCHAR2(32 BYTE)                 NOT NULL,
  USRIN_NUM   NUMBER(12)                        NOT NULL,
  USR_NAME    VARCHAR2(64 BYTE)                 NOT NULL,
  SESSION_ID  VARCHAR2(32 BYTE)                 NOT NULL,
  REM_CLIENT  VARCHAR2(1000 BYTE)               NOT NULL,
  ASTATUS     VARCHAR2(200 BYTE)                NOT NULL,
  USRIN_DATE  DATE                              NOT NULL
)
TABLESPACE BIOSYS_DT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          3M
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOLOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE BIOSYS.USRIN$LOG IS 'Лог входов в систему';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.REM_ADDR IS 'Адрес пользователя';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.USRIN_NUM IS 'Номер входа';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.USR_NAME IS 'Логин';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.SESSION_ID IS 'ID Сессии';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.REM_CLIENT IS 'Клиент пользователя';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.ASTATUS IS 'Статус';

COMMENT ON COLUMN BIOSYS.USRIN$LOG.USRIN_DATE IS 'Дата входа';



ALTER TABLE BIOSYS.USRS
 DROP PRIMARY KEY CASCADE;

DROP TABLE BIOSYS.USRS CASCADE CONSTRAINTS;

CREATE TABLE BIOSYS.USRS
(
  USR_UID     VARCHAR2(32 BYTE)                 NOT NULL,
  ODEP_UID    VARCHAR2(32 BYTE),
  CON_UID     VARCHAR2(32 BYTE),
  USR_NAME    VARCHAR2(64 BYTE)                 NOT NULL,
  USR_PWD     VARCHAR2(32 BYTE)                 NOT NULL,
  USR_FAM     VARCHAR2(100 BYTE)                NOT NULL,
  USR_FNAME   VARCHAR2(100 BYTE)                NOT NULL,
  USR_SNAME   VARCHAR2(100 BYTE)                NOT NULL,
  REG_DATE    DATE                              NOT NULL,
  BLOCKED     NUMBER(1)                         DEFAULT 0                     NOT NULL,
  EMAIL_ADDR  VARCHAR2(100 BYTE)                NOT NULL,
  USR_PHONE   VARCHAR2(12 BYTE),
  CONFIRMED   NUMBER(1)                         DEFAULT 0,
  GARBAGED    NUMBER(1)                         DEFAULT 0                     NOT NULL,
  EXTINFO     VARCHAR2(1000 BYTE)
)
TABLESPACE BIOSYS_DT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOLOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE BIOSYS.USRS IS 'Пользователи';

COMMENT ON COLUMN BIOSYS.USRS.USR_UID IS 'UID пользователя';

COMMENT ON COLUMN BIOSYS.USRS.ODEP_UID IS 'UID отдела';

COMMENT ON COLUMN BIOSYS.USRS.CON_UID IS 'UID соединения';

COMMENT ON COLUMN BIOSYS.USRS.USR_NAME IS 'Логин';

COMMENT ON COLUMN BIOSYS.USRS.USR_PWD IS 'Пароль';

COMMENT ON COLUMN BIOSYS.USRS.USR_FAM IS 'Фамилия';

COMMENT ON COLUMN BIOSYS.USRS.USR_FNAME IS 'Имя';

COMMENT ON COLUMN BIOSYS.USRS.USR_SNAME IS 'Отчество';

COMMENT ON COLUMN BIOSYS.USRS.REG_DATE IS 'Дата регистрации';

COMMENT ON COLUMN BIOSYS.USRS.BLOCKED IS 'Заблокирован';

COMMENT ON COLUMN BIOSYS.USRS.EMAIL_ADDR IS 'e-mail';

COMMENT ON COLUMN BIOSYS.USRS.USR_PHONE IS 'Телефон';

COMMENT ON COLUMN BIOSYS.USRS.CONFIRMED IS 'Подтверждение';

COMMENT ON COLUMN BIOSYS.USRS.GARBAGED IS 'В мусор';



CREATE INDEX BIOSYS.IX_USRS_GBRG ON BIOSYS.USRS
(GARBAGED)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX BIOSYS.PK_DBCONS ON BIOSYS.DBCONS
(CON_UID)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX BIOSYS.PK_RADDRSS ON BIOSYS.RADDRSS
(REM_ADDR)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX BIOSYS.PK_SMTP$LOG ON BIOSYS.SMTP$LOG
(ERR_UID)
NOLOGGING
TABLESPACE BIOSYS_DT
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX BIOSYS.PK_UROLES ON BIOSYS.UROLES
(ROLE_UID)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX BIOSYS.PK_USRIN$LOG ON BIOSYS.USRIN$LOG
(REM_ADDR, USRIN_NUM)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          2M
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX BIOSYS.PK_USRS ON BIOSYS.USRS
(USR_UID)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


ALTER TABLE BIOSYS.ORGSTRUCT
 DROP PRIMARY KEY CASCADE;

DROP TABLE BIOSYS.ORGSTRUCT CASCADE CONSTRAINTS;

CREATE TABLE BIOSYS.ORGSTRUCT
(
  ODEP_UID       VARCHAR2(32 BYTE)              NOT NULL,
  ODEP_PRNT      VARCHAR2(32 BYTE),
  ODEP_NAME      VARCHAR2(500 BYTE)             NOT NULL,
  ODEP_UID_AUTO  NUMBER(1)                      DEFAULT 1                     NOT NULL,
  OWNER_UID      VARCHAR2(32 BYTE),
  ODEP_DESC      VARCHAR2(1000 BYTE)
)
TABLESPACE BIOSYS_DT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
LOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE BIOSYS.ORGSTRUCT IS 'Орг. структура';

COMMENT ON COLUMN BIOSYS.ORGSTRUCT.ODEP_UID IS 'UID организации/отдела';

COMMENT ON COLUMN BIOSYS.ORGSTRUCT.ODEP_PRNT IS 'Ссылка на вышестоящий отдел/организацию';

COMMENT ON COLUMN BIOSYS.ORGSTRUCT.ODEP_NAME IS 'Название отдела/организации';

COMMENT ON COLUMN BIOSYS.ORGSTRUCT.ODEP_UID_AUTO IS 'UID устанавливается автоматом';

COMMENT ON COLUMN BIOSYS.ORGSTRUCT.OWNER_UID IS 'UID пользователя - владельца';

COMMENT ON COLUMN BIOSYS.ORGSTRUCT.ODEP_DESC IS 'Описание отдела/организации';



ALTER TABLE BIOSYS.USRRLES
 DROP PRIMARY KEY CASCADE;

DROP TABLE BIOSYS.USRRLES CASCADE CONSTRAINTS;

CREATE TABLE BIOSYS.USRRLES
(
  ROLE_UID  VARCHAR2(64 BYTE)                   NOT NULL,
  USR_UID   VARCHAR2(32 BYTE)                   NOT NULL
)
TABLESPACE BIOSYS_DT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOLOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE BIOSYS.USRRLES IS 'Роли пользователя';

COMMENT ON COLUMN BIOSYS.USRRLES.ROLE_UID IS 'UID роли';

COMMENT ON COLUMN BIOSYS.USRRLES.USR_UID IS 'UID пользователя';



CREATE UNIQUE INDEX BIOSYS.PK_ORGSTRUCT ON BIOSYS.ORGSTRUCT
(ODEP_UID)
LOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX BIOSYS.PK_USRRLES ON BIOSYS.USRRLES
(ROLE_UID, USR_UID)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


ALTER TABLE BIOSYS.DBCONS ADD (
  CONSTRAINT CKC_CON_UID_DBCONS
  CHECK (CON_UID = upper(CON_UID)),
  CONSTRAINT CKC_IS_DEFAULT_DBCONS
  CHECK (IS_DEFAULT in (0,1)),
  CONSTRAINT PK_DBCONS
  PRIMARY KEY
  (CON_UID)
  USING INDEX BIOSYS.PK_DBCONS);

ALTER TABLE BIOSYS.RADDRSS ADD (
  CONSTRAINT PK_RADDRSS
  PRIMARY KEY
  (REM_ADDR)
  USING INDEX BIOSYS.PK_RADDRSS);

ALTER TABLE BIOSYS.SMTP$LOG ADD (
  CONSTRAINT CKC_ERR_UID_SMTP$LOG
  CHECK (ERR_UID = upper(ERR_UID)),
  CONSTRAINT PK_SMTP$LOG
  PRIMARY KEY
  (ERR_UID)
  USING INDEX BIOSYS.PK_SMTP$LOG);

ALTER TABLE BIOSYS.UROLES ADD (
  CONSTRAINT CKC_ROLE_IS_SYS_UROLES
  CHECK (ROLE_IS_SYS in (0,1)),
  CONSTRAINT CKC_ROLE_UID_UROLES
  CHECK (ROLE_UID = upper(ROLE_UID)),
  CONSTRAINT PK_UROLES
  PRIMARY KEY
  (ROLE_UID)
  USING INDEX BIOSYS.PK_UROLES);

ALTER TABLE BIOSYS.USRIN$LOG ADD (
  CONSTRAINT CKC_SESSION_ID_USRIN$LO
  CHECK (SESSION_ID = upper(SESSION_ID)),
  CONSTRAINT PK_USRIN$LOG
  PRIMARY KEY
  (REM_ADDR, USRIN_NUM)
  USING INDEX BIOSYS.PK_USRIN$LOG);

ALTER TABLE BIOSYS.USRS ADD (
  CONSTRAINT CKC_BLOCKED_USRS
  CHECK (BLOCKED in (0,1)),
  CONSTRAINT CKC_CONFIRMED_USRS
  CHECK (CONFIRMED is null or (CONFIRMED in (0,1))),
  CONSTRAINT CKC_CON_UID_USRS
  CHECK (CON_UID is null or (CON_UID = upper(CON_UID))),
  CONSTRAINT CKC_GARBAGED_USRS
  CHECK (GARBAGED in (0,1)),
  CONSTRAINT CKC_ODEP_UID_USRS
  CHECK (ODEP_UID is null or (ODEP_UID = upper(ODEP_UID))),
  CONSTRAINT CKC_USR_FAM_USRS
  CHECK (USR_FAM = upper(USR_FAM)),
  CONSTRAINT CKC_USR_FNAME_USRS
  CHECK (USR_FNAME = upper(USR_FNAME)),
  CONSTRAINT CKC_USR_NAME_USRS
  CHECK (USR_NAME = upper(USR_NAME)),
  CONSTRAINT CKC_USR_SNAME_USRS
  CHECK (USR_SNAME = upper(USR_SNAME)),
  CONSTRAINT CKC_USR_UID_USRS
  CHECK (USR_UID = upper(USR_UID)),
  CONSTRAINT PK_USRS
  PRIMARY KEY
  (USR_UID)
  USING INDEX BIOSYS.PK_USRS);

ALTER TABLE BIOSYS.ORGSTRUCT ADD (
  CONSTRAINT CKC_ODEP_PRNT_ORGSTRUC
  CHECK (ODEP_PRNT is null or (ODEP_PRNT = upper(ODEP_PRNT))),
  CONSTRAINT CKC_ODEP_UID_AUTO_ORGSTRUC
  CHECK (ODEP_UID_AUTO in (0,1)),
  CONSTRAINT CKC_ODEP_UID_ORGSTRUC
  CHECK (ODEP_UID = upper(ODEP_UID)),
  CONSTRAINT CKC_OWNER_UID_ORGSTRUC
  CHECK (OWNER_UID is null or (OWNER_UID = upper(OWNER_UID))),
  CONSTRAINT PK_ORGSTRUCT
  PRIMARY KEY
  (ODEP_UID)
  USING INDEX BIOSYS.PK_ORGSTRUCT);

ALTER TABLE BIOSYS.USRRLES ADD (
  CONSTRAINT CKC_ROLE_UID_USRRLES
  CHECK (ROLE_UID = upper(ROLE_UID)),
  CONSTRAINT CKC_USR_UID_USRRLES
  CHECK (USR_UID = upper(USR_UID)),
  CONSTRAINT PK_USRRLES
  PRIMARY KEY
  (ROLE_UID, USR_UID)
  USING INDEX BIOSYS.PK_USRRLES);

ALTER TABLE BIOSYS.USRIN$LOG ADD (
  CONSTRAINT FK_USRIN$LO_RF7_RADDRSS 
  FOREIGN KEY (REM_ADDR) 
  REFERENCES BIOSYS.RADDRSS (REM_ADDR));

ALTER TABLE BIOSYS.USRS ADD (
  CONSTRAINT FK_USRS_RF6_DBCONS 
  FOREIGN KEY (CON_UID) 
  REFERENCES BIOSYS.DBCONS (CON_UID));

ALTER TABLE BIOSYS.ORGSTRUCT ADD (
  CONSTRAINT FK_ORGSTRUC_RF1_ORGSTRUC 
  FOREIGN KEY (ODEP_PRNT) 
  REFERENCES BIOSYS.ORGSTRUCT (ODEP_UID),
  CONSTRAINT FK_ORGSTRUC_RF2_USRS 
  FOREIGN KEY (OWNER_UID) 
  REFERENCES BIOSYS.USRS (USR_UID));

ALTER TABLE BIOSYS.USRRLES ADD (
  CONSTRAINT FK_USRRLES_RF8_UROLES 
  FOREIGN KEY (ROLE_UID) 
  REFERENCES BIOSYS.UROLES (ROLE_UID),
  CONSTRAINT FK_USRRLES_RF9_USRS 
  FOREIGN KEY (USR_UID) 
  REFERENCES BIOSYS.USRS (USR_UID));

GRANT REFERENCES, SELECT ON BIOSYS.USRS TO GIVC_REG;
DROP INDEX BIOSYS.IX_USRS_GBRG;

CREATE INDEX BIOSYS.IX_USRS_GBRG ON BIOSYS.USRS
(GARBAGED)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP INDEX BIOSYS.PK_DBCONS;

CREATE UNIQUE INDEX BIOSYS.PK_DBCONS ON BIOSYS.DBCONS
(CON_UID)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP INDEX BIOSYS.PK_ORGSTRUCT;

CREATE UNIQUE INDEX BIOSYS.PK_ORGSTRUCT ON BIOSYS.ORGSTRUCT
(ODEP_UID)
LOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP INDEX BIOSYS.PK_RADDRSS;

CREATE UNIQUE INDEX BIOSYS.PK_RADDRSS ON BIOSYS.RADDRSS
(REM_ADDR)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP INDEX BIOSYS.PK_SMTP$LOG;

CREATE UNIQUE INDEX BIOSYS.PK_SMTP$LOG ON BIOSYS.SMTP$LOG
(ERR_UID)
NOLOGGING
TABLESPACE BIOSYS_DT
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP INDEX BIOSYS.PK_UROLES;

CREATE UNIQUE INDEX BIOSYS.PK_UROLES ON BIOSYS.UROLES
(ROLE_UID)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP INDEX BIOSYS.PK_USRIN$LOG;

CREATE UNIQUE INDEX BIOSYS.PK_USRIN$LOG ON BIOSYS.USRIN$LOG
(REM_ADDR, USRIN_NUM)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          2M
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP INDEX BIOSYS.PK_USRRLES;

CREATE UNIQUE INDEX BIOSYS.PK_USRRLES ON BIOSYS.USRRLES
(ROLE_UID, USR_UID)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP INDEX BIOSYS.PK_USRS;

CREATE UNIQUE INDEX BIOSYS.PK_USRS ON BIOSYS.USRS
(USR_UID)
NOLOGGING
TABLESPACE BIOSYS_INDX
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;
DROP TYPE BIOSYS.JSON;

CREATE OR REPLACE TYPE BIOSYS.json as object (
  /*
  Copyright (c) 2010 Jonas Krogsboell

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
  */

  /* Variables */
  json_data json_value_array,
  check_for_duplicate number,

  /* Constructors */
  constructor function json return self as result,
  constructor function json(str varchar2) return self as result,
  constructor function json(str in clob) return self as result,
  constructor function json(cast json_value) return self as result,
  constructor function json(l in out nocopy json_list) return self as result,

  /* Member setter methods */
  member procedure remove(pair_name varchar2),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json_value, position pls_integer default null),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value varchar2, position pls_integer default null),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value number, position pls_integer default null),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value boolean, position pls_integer default null),
  member procedure check_duplicate(self in out nocopy json, set boolean),
  member procedure remove_duplicates(self in out nocopy json),

  /* deprecated putter use json_value */
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json, position pls_integer default null),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json_list, position pls_integer default null),

  /* Member getter methods */
  member function count return number,
  member function get(pair_name varchar2) return json_value,
  member function get(position pls_integer) return json_value,
  member function index_of(pair_name varchar2) return number,
  member function exist(pair_name varchar2) return boolean,

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2,
  member procedure to_clob(self in json, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0),
  member procedure print(self in json, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null), --32512 is maximum
  member procedure htp(self in json, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null),

  member function to_json_value return json_value,
  /* json path */
  member function path(json_path varchar2) return json_value,

  /* json path_put */
  member procedure path_put(self in out nocopy json, json_path varchar2, elem json_value),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem varchar2),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem number),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem boolean),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem json_list),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem json),

  /* map functions */
  member function get_values return json_list,
  member function get_keys return json_list

) not final;
/


DROP TYPE BIOSYS.JSON_LIST;

CREATE OR REPLACE TYPE BIOSYS.json_list as object (
  /*
  Copyright (c) 2010 Jonas Krogsboell

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
  */

  list_data json_value_array,
  constructor function json_list return self as result,
  constructor function json_list(str varchar2) return self as result,
  constructor function json_list(str clob) return self as result,
  constructor function json_list(cast json_value) return self as result,

  member procedure add_elem(self in out nocopy json_list, elem json_value, position pls_integer default null),
  member procedure add_elem(self in out nocopy json_list, elem varchar2, position pls_integer default null),
  member procedure add_elem(self in out nocopy json_list, elem number, position pls_integer default null),
  member procedure add_elem(self in out nocopy json_list, elem boolean, position pls_integer default null),

  /* deprecated putter use json_value */
  member procedure add_elem(self in out nocopy json_list, elem json_list, position pls_integer default null),

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem json_value),
  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem varchar2),
  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem number),
  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem boolean),
  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem json_list),

  member function count return number,
  member procedure remove_elem(self in out nocopy json_list, position pls_integer),
  member procedure remove_first(self in out nocopy json_list),
  member procedure remove_last(self in out nocopy json_list),
  member function get_elem(position pls_integer) return json_value,
  member function get_first return json_value,
  member function get_last return json_value,

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2,
  member procedure to_clob(self in json_list, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0),
  member procedure print(self in json_list, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null), --32512 is maximum
  member procedure htp(self in json_list, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null),

  /* json path */
  member function path(json_path varchar2) return json_value,
  /* json path_put */
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem json_value),
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem varchar2),
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem number),
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem boolean),
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem json_list),

  member function to_json_value return json_value
) not final;
/


DROP TYPE BIOSYS.JSON_VALUE;

CREATE OR REPLACE TYPE BIOSYS.json_value as object
(
  /*
  Copyright (c) 2010 Jonas Krogsboell

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
  */

  typeval number(1), /* 1 = object, 2 = array, 3 = string, 4 = number, 5 = bool, 6 = null */
  str varchar2(4000),
  num number, /* store 1 as true, 0 as false */
  object_or_array anydata, /* object or array in here */

  /* mapping */
  mapname varchar2(4000),
  mapindx number(32),

  constructor function json_value(object_or_array anydata) return self as result,
  constructor function json_value(str varchar2, esc boolean default true) return self as result,
  constructor function json_value(num number) return self as result,
  constructor function json_value(b boolean) return self as result,
  constructor function json_value return self as result,
  static function makenull return json_value,

  member function get_type return varchar2,
  member function get_string return varchar2,
  member function get_number return number,
  member function get_bool return boolean,
  member function get_null return varchar2,

  member function is_object return boolean,
  member function is_array return boolean,
  member function is_string return boolean,
  member function is_number return boolean,
  member function is_bool return boolean,
  member function is_null return boolean,

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2,
  member procedure to_clob(self in json_value, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0),
  member procedure print(self in json_value, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null), --32512 is maximum
  member procedure htp(self in json_value, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null),

  member function value_of(self in json_value) return varchar2

) not final;
/


DROP TYPE BIOSYS.JSON_VALUE_ARRAY;

CREATE OR REPLACE TYPE BIOSYS.json_value_array as table of json_value;
/


DROP TYPE BODY BIOSYS.JSON;

CREATE OR REPLACE TYPE BODY BIOSYS.json as

  /* Constructors */
  constructor function json return self as result as
  begin
    self.json_data := json_value_array();
    self.check_for_duplicate := 1;
    return;
  end;

  constructor function json(str varchar2) return self as result as
  begin
    self := json_parser.parser(str);
    self.check_for_duplicate := 1;
    return;
  end;

  constructor function json(str in clob) return self as result as
  begin
    self := json_parser.parser(str);
    self.check_for_duplicate := 1;
    return;
  end;

  constructor function json(cast json_value) return self as result as
    x number;
  begin
    x := cast.object_or_array.getobject(self);
    self.check_for_duplicate := 1;
    return;
  end;

  constructor function json(l in out nocopy json_list) return self as result as
  begin
    for i in 1 .. l.list_data.count loop
      if(l.list_data(i).mapname is null or l.list_data(i).mapname like 'row%') then
      l.list_data(i).mapname := 'row'||i;
      end if;
      l.list_data(i).mapindx := i;
    end loop;

    self.json_data := l.list_data;
    self.check_for_duplicate := 1;
    return;
  end;

  /* Member setter methods */
  member procedure remove(self in out nocopy json, pair_name varchar2) as
    temp json_value;
    indx pls_integer;

    function get_member(pair_name varchar2) return json_value as
      indx pls_integer;
    begin
      indx := json_data.first;
      loop
        exit when indx is null;
        if(pair_name is null and json_data(indx).mapname is null) then return json_data(indx); end if;
        if(json_data(indx).mapname = pair_name) then return json_data(indx); end if;
        indx := json_data.next(indx);
      end loop;
      return null;
    end;
  begin
    temp := get_member(pair_name);
    if(temp is null) then return; end if;

    indx := json_data.next(temp.mapindx);
    loop
      exit when indx is null;
      json_data(indx).mapindx := indx - 1;
      json_data(indx-1) := json_data(indx);
      indx := json_data.next(indx);
    end loop;
    json_data.trim(1);
    --num_elements := num_elements - 1;
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json_value, position pls_integer default null) as
    insert_value json_value := nvl(pair_value, json_value.makenull);
    indx pls_integer; x number;
    temp json_value;
    function get_member(pair_name varchar2) return json_value as
      indx pls_integer;
    begin
      indx := json_data.first;
      loop
        exit when indx is null;
        if(pair_name is null and json_data(indx).mapname is null) then return json_data(indx); end if;
        if(json_data(indx).mapname = pair_name) then return json_data(indx); end if;
        indx := json_data.next(indx);
      end loop;
      return null;
    end;
  begin
    --dbms_output.put_line('PN '||pair_name);

--    if(pair_name is null) then
--      raise_application_error(-20102, 'JSON put-method type error: name cannot be null');
--    end if;
    insert_value.mapname := pair_name;
--    self.remove(pair_name);
    if(self.check_for_duplicate = 1) then temp := get_member(pair_name); else temp := null; end if;
    if(temp is not null) then
      insert_value.mapindx := temp.mapindx;
      json_data(temp.mapindx) := insert_value;
      return;
    elsif(position is null or position > self.count) then
      --insert at the end of the list
      --dbms_output.put_line('Test');
--      indx := self.count + 1;
      json_data.extend(1);
      json_data(json_data.count) := insert_value;
--      insert_value.mapindx := json_data.count;
      json_data(json_data.count).mapindx := json_data.count;
--      dbms_output.put_line('Test2'||insert_value.mapindx);
--      dbms_output.put_line('Test2'||insert_value.mapname);
--      insert_value.print(false);
--      self.print;
    elsif(position < 2) then
      --insert at the start of the list
      indx := json_data.last;
      json_data.extend;
      loop
        exit when indx is null;
        temp := json_data(indx);
        temp.mapindx := indx+1;
        json_data(temp.mapindx) := temp;
        indx := json_data.prior(indx);
      end loop;
      json_data(1) := insert_value;
      insert_value.mapindx := 1;
    else
      --insert somewhere in the list
      indx := json_data.last;
--      dbms_output.put_line('Test '||indx);
      json_data.extend;
--      dbms_output.put_line('Test '||indx);
      loop
--        dbms_output.put_line('Test '||indx);
        temp := json_data(indx);
        temp.mapindx := indx + 1;
        json_data(temp.mapindx) := temp;
        exit when indx = position;
        indx := json_data.prior(indx);
      end loop;
      json_data(position) := insert_value;
      json_data(position).mapindx := position;
    end if;
--    num_elements := num_elements + 1;
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value varchar2, position pls_integer default null) as
  begin
    put(pair_name, json_value(pair_value), position);
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value number, position pls_integer default null) as
  begin
    if(pair_value is null) then
      put(pair_name, json_value(), position);
    else
      put(pair_name, json_value(pair_value), position);
    end if;
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value boolean, position pls_integer default null) as
  begin
    if(pair_value is null) then
      put(pair_name, json_value(), position);
    else
      put(pair_name, json_value(pair_value), position);
    end if;
  end;

  member procedure check_duplicate(self in out nocopy json, set boolean) as
  begin
    if(set) then
      check_for_duplicate := 1;
    else
      check_for_duplicate := 0;
    end if;
  end;

  /* deprecated putters */

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json, position pls_integer default null) as
  begin
    if(pair_value is null) then
      put(pair_name, json_value(), position);
    else
      put(pair_name, pair_value.to_json_value, position);
    end if;
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json_list, position pls_integer default null) as
  begin
    if(pair_value is null) then
      put(pair_name, json_value(), position);
    else
      put(pair_name, pair_value.to_json_value, position);
    end if;
  end;

  /* Member getter methods */
  member function count return number as
  begin
    return self.json_data.count;
  end;

  member function get(pair_name varchar2) return json_value as
    indx pls_integer;
  begin
    indx := json_data.first;
    loop
      exit when indx is null;
      if(pair_name is null and json_data(indx).mapname is null) then return json_data(indx); end if;
      if(json_data(indx).mapname = pair_name) then return json_data(indx); end if;
      indx := json_data.next(indx);
    end loop;
    return null;
  end;

  member function get(position pls_integer) return json_value as
  begin
    if(self.count >= position and position > 0) then
      return self.json_data(position);
    end if;
    return null; -- do not throw error, just return null
  end;

  member function index_of(pair_name varchar2) return number as
    indx pls_integer;
  begin
    indx := json_data.first;
    loop
      exit when indx is null;
      if(pair_name is null and json_data(indx).mapname is null) then return indx; end if;
      if(json_data(indx).mapname = pair_name) then return indx; end if;
      indx := json_data.next(indx);
    end loop;
    return -1;
  end;

  member function exist(pair_name varchar2) return boolean as
  begin
    return (self.get(pair_name) is not null);
  end;

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2 as
  begin
    if(spaces is null) then
      return json_printer.pretty_print(self, line_length => chars_per_line);
    else
      return json_printer.pretty_print(self, spaces, line_length => chars_per_line);
    end if;
  end;

  member procedure to_clob(self in json, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0) as
  begin
    if(spaces is null) then
      json_printer.pretty_print(self, false, buf, line_length => chars_per_line);
    else
      json_printer.pretty_print(self, spaces, buf, line_length => chars_per_line);
    end if;
  end;

  member procedure print(self in json, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null) as --32512 is the real maximum in sqldeveloper
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.dbms_output_clob(my_clob, json_printer.newline_char, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member procedure htp(self in json, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null) as
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.htp_output_clob(my_clob, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member function to_json_value return json_value as
  begin
    return json_value(anydata.convertobject(self));
  end;

  /* json path */
  member function path(json_path varchar2) return json_value as
  begin
    return json_ext.get_json_value(self, json_path);
  end path;

  /* json path_put */
  member procedure path_put(self in out nocopy json, json_path varchar2, elem json_value) as
  begin
    json_ext.put(self, json_path, elem);
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem varchar2) as
  begin
    json_ext.put(self, json_path, elem);
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem number) as
  begin
    if(elem is null) then
      json_ext.put(self, json_path, json_value());
    else
      json_ext.put(self, json_path, elem);
    end if;
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem boolean) as
  begin
    if(elem is null) then
      json_ext.put(self, json_path, json_value());
    else
      json_ext.put(self, json_path, elem);
    end if;
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem json_list) as
  begin
    if(elem is null) then
      json_ext.put(self, json_path, json_value());
    else
      json_ext.put(self, json_path, elem);
    end if;
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem json) as
  begin
    if(elem is null) then
      json_ext.put(self, json_path, json_value());
    else
      json_ext.put(self, json_path, elem);
    end if;
  end path_put;

  /* Thanks to Matt Nolan */
  member function get_keys return json_list as
    keys json_list;
    indx pls_integer;
  begin
    keys := json_list();
    indx := json_data.first;
    loop
      exit when indx is null;
      keys.add_elem(json_data(indx).mapname);
      indx := json_data.next(indx);
    end loop;
    return keys;
  end;

  member function get_values return json_list as
    vals json_list := json_list();
  begin
    vals.list_data := self.json_data;
    return vals;
  end;

  member procedure remove_duplicates(self in out nocopy json) as
  begin
    json_parser.remove_duplicates(self);
  end remove_duplicates;


end;
/


DROP TYPE BODY BIOSYS.JSON_LIST;

CREATE OR REPLACE TYPE BODY BIOSYS.json_list as

  constructor function json_list return self as result as
  begin
    self.list_data := json_value_array();
    return;
  end;

  constructor function json_list(str varchar2) return self as result as
  begin
    self := json_parser.parse_list(str);
    return;
  end;

  constructor function json_list(str clob) return self as result as
  begin
    self := json_parser.parse_list(str);
    return;
  end;

  constructor function json_list(cast json_value) return self as result as
    x number;
  begin
    x := cast.object_or_array.getobject(self);
    return;
  end;


  member procedure add_elem(self in out nocopy json_list, elem json_value, position pls_integer default null) as
    indx pls_integer;
    insert_value json_value := NVL(elem, json_value);
  begin
    if(position is null or position > self.count) then --end of list
      indx := self.count + 1;
      self.list_data.extend(1);
      self.list_data(indx) := insert_value;
    elsif(position < 1) then --new first
      indx := self.count;
      self.list_data.extend(1);
      for x in reverse 1 .. indx loop
        self.list_data(x+1) := self.list_data(x);
      end loop;
      self.list_data(1) := insert_value;
    else
      indx := self.count;
      self.list_data.extend(1);
      for x in reverse position .. indx loop
        self.list_data(x+1) := self.list_data(x);
      end loop;
      self.list_data(position) := insert_value;
    end if;

  end;

  member procedure add_elem(self in out nocopy json_list, elem varchar2, position pls_integer default null) as
  begin
    add_elem(json_value(elem), position);
  end;

  member procedure add_elem(self in out nocopy json_list, elem number, position pls_integer default null) as
  begin
    if(elem is null) then
      add_elem(json_value(), position);
    else
      add_elem(json_value(elem), position);
    end if;
  end;

  member procedure add_elem(self in out nocopy json_list, elem boolean, position pls_integer default null) as
  begin
    if(elem is null) then
      add_elem(json_value(), position);
    else
      add_elem(json_value(elem), position);
    end if;
  end;

  member procedure add_elem(self in out nocopy json_list, elem json_list, position pls_integer default null) as
  begin
    if(elem is null) then
      add_elem(json_value(), position);
    else
      add_elem(elem.to_json_value, position);
    end if;
  end;

 member procedure set_elem(self in out nocopy json_list, position pls_integer, elem json_value) as
    insert_value json_value := NVL(elem, json_value);
    indx number;
  begin
    if(position > self.count) then --end of list
      indx := self.count + 1;
      self.list_data.extend(1);
      self.list_data(indx) := insert_value;
    elsif(position < 1) then --maybe an error message here
      null;
    else
      self.list_data(position) := insert_value;
    end if;
  end;

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem varchar2) as
  begin
    set_elem(position, json_value(elem));
  end;

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem number) as
  begin
    if(elem is null) then
      set_elem(position, json_value());
    else
      set_elem(position, json_value(elem));
    end if;
  end;

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem boolean) as
  begin
    if(elem is null) then
      set_elem(position, json_value());
    else
      set_elem(position, json_value(elem));
    end if;
  end;

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem json_list) as
  begin
    if(elem is null) then
      set_elem(position, json_value());
    else
      set_elem(position, elem.to_json_value);
    end if;
  end;

  member function count return number as
  begin
    return self.list_data.count;
  end;

  member procedure remove_elem(self in out nocopy json_list, position pls_integer) as
  begin
    if(position is null or position < 1 or position > self.count) then return; end if;
    for x in (position+1) .. self.count loop
      self.list_data(x-1) := self.list_data(x);
    end loop;
    self.list_data.trim(1);
  end;

  member procedure remove_first(self in out nocopy json_list) as
  begin
    for x in 2 .. self.count loop
      self.list_data(x-1) := self.list_data(x);
    end loop;
    if(self.count > 0) then
      self.list_data.trim(1);
    end if;
  end;

  member procedure remove_last(self in out nocopy json_list) as
  begin
    if(self.count > 0) then
      self.list_data.trim(1);
    end if;
  end;

  member function get_elem(position pls_integer) return json_value as
  begin
    if(self.count >= position and position > 0) then
      return self.list_data(position);
    end if;
    return null; -- do not throw error, just return null
  end;

  member function get_first return json_value as
  begin
    if(self.count > 0) then
      return self.list_data(self.list_data.first);
    end if;
    return null; -- do not throw error, just return null
  end;

  member function get_last return json_value as
  begin
    if(self.count > 0) then
      return self.list_data(self.list_data.last);
    end if;
    return null; -- do not throw error, just return null
  end;

  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2 as
  begin
    if(spaces is null) then
      return json_printer.pretty_print_list(self, line_length => chars_per_line);
    else
      return json_printer.pretty_print_list(self, spaces, line_length => chars_per_line);
    end if;
  end;

  member procedure to_clob(self in json_list, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0) as
  begin
    if(spaces is null) then
      json_printer.pretty_print_list(self, false, buf, line_length => chars_per_line);
    else
      json_printer.pretty_print_list(self, spaces, buf, line_length => chars_per_line);
    end if;
  end;

  member procedure print(self in json_list, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null) as --32512 is the real maximum in sqldeveloper
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print_list(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.dbms_output_clob(my_clob, json_printer.newline_char, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member procedure htp(self in json_list, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null) as
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print_list(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.htp_output_clob(my_clob, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  /* json path */
  member function path(json_path varchar2) return json_value as
    cp json_list := self;
  begin
    return json_ext.get_json_value(json(cp), json_path);
  end path;


  /* json path_put */
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem json_value) as
    objlist json := json(self);
  begin
    json_ext.put(objlist, json_path, elem);
    self := objlist.get_values;
  end path_put;

  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem varchar2) as
    objlist json := json(self);
  begin
    json_ext.put(objlist, json_path, elem);
    self := objlist.get_values;
  end path_put;

  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem number) as
    objlist json := json(self);
  begin
    if(elem is null) then
      json_ext.put(objlist, json_path, json_value);
    else
      json_ext.put(objlist, json_path, elem);
    end if;
    self := objlist.get_values;
  end path_put;

  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem boolean) as
    objlist json := json(self);
  begin
    if(elem is null) then
      json_ext.put(objlist, json_path, json_value);
    else
      json_ext.put(objlist, json_path, elem);
    end if;
    self := objlist.get_values;
  end path_put;

  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem json_list) as
    objlist json := json(self);
  begin
    if(elem is null) then
      json_ext.put(objlist, json_path, json_value);
    else
      json_ext.put(objlist, json_path, elem);
    end if;
    self := objlist.get_values;
  end path_put;

  member function to_json_value return json_value as
  begin
    return json_value(anydata.convertobject(self));
  end;

end;
/


DROP TYPE BODY BIOSYS.JSON_VALUE;

CREATE OR REPLACE TYPE BODY BIOSYS.json_value as

  constructor function json_value(object_or_array anydata) return self as result as
  begin
    case object_or_array.gettypename
      when sys_context('userenv', 'current_schema')||'.JSON_LIST' then self.typeval := 2;
      when sys_context('userenv', 'current_schema')||'.JSON' then self.typeval := 1;
      else raise_application_error(-20102, 'JSON_Value init error (JSON or JSON\_List allowed)');
    end case;
    self.object_or_array := object_or_array;
    if(self.object_or_array is null) then self.typeval := 6; end if;

    return;
  end json_value;

  constructor function json_value(str varchar2, esc boolean default true) return self as result as
  begin
    self.typeval := 3;
    if(esc) then self.num := 1; else self.num := 0; end if; --message to pretty printer
    self.str := str;
    return;
  end json_value;

  constructor function json_value(num number) return self as result as
  begin
    self.typeval := 4;
    self.num := num;
    if(self.num is null) then self.typeval := 6; end if;
    return;
  end json_value;

  constructor function json_value(b boolean) return self as result as
  begin
    self.typeval := 5;
    self.num := 0;
    if(b) then self.num := 1; end if;
    if(b is null) then self.typeval := 6; end if;
    return;
  end json_value;

  constructor function json_value return self as result as
  begin
    self.typeval := 6; /* for JSON null */
    return;
  end json_value;

  static function makenull return json_value as
  begin
    return json_value;
  end makenull;

  member function get_type return varchar2 as
  begin
    case self.typeval
    when 1 then return 'object';
    when 2 then return 'array';
    when 3 then return 'string';
    when 4 then return 'number';
    when 5 then return 'bool';
    when 6 then return 'null';
    end case;

    return 'unknown type';
  end get_type;

  member function get_string return varchar2 as
  begin
    if(self.typeval = 3) then
      return self.str;
    end if;
    return null;
  end get_string;

  member function get_number return number as
  begin
    if(self.typeval = 4) then
      return self.num;
    end if;
    return null;
  end get_number;

  member function get_bool return boolean as
  begin
    if(self.typeval = 5) then
      return self.num = 1;
    end if;
    return null;
  end get_bool;

  member function get_null return varchar2 as
  begin
    if(self.typeval = 6) then
      return 'null';
    end if;
    return null;
  end get_null;

  member function is_object return boolean as begin return self.typeval = 1; end;
  member function is_array return boolean as begin return self.typeval = 2; end;
  member function is_string return boolean as begin return self.typeval = 3; end;
  member function is_number return boolean as begin return self.typeval = 4; end;
  member function is_bool return boolean as begin return self.typeval = 5; end;
  member function is_null return boolean as begin return self.typeval = 6; end;

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2 as
  begin
    if(spaces is null) then
      return json_printer.pretty_print_any(self, line_length => chars_per_line);
    else
      return json_printer.pretty_print_any(self, spaces, line_length => chars_per_line);
    end if;
  end;

  member procedure to_clob(self in json_value, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0) as
  begin
    if(spaces is null) then
      json_printer.pretty_print_any(self, false, buf, line_length => chars_per_line);
    else
      json_printer.pretty_print_any(self, spaces, buf, line_length => chars_per_line);
    end if;
  end;

  member procedure print(self in json_value, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null) as --32512 is the real maximum in sqldeveloper
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print_any(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.dbms_output_clob(my_clob, json_printer.newline_char, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member procedure htp(self in json_value, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null) as
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print_any(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.htp_output_clob(my_clob, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member function value_of(self in json_value) return varchar2 as
  begin
    case self.typeval
    when 1 then return 'json object';
    when 2 then return 'json array';
    when 3 then return self.get_string();
    when 4 then return self.get_number();
    when 5 then if(self.get_bool()) then return 'true'; else return 'false'; end if;
    else return null;
    end case;
  end;


end;
/


GRANT EXECUTE ON BIOSYS.JSON TO PUBLIC;

GRANT EXECUTE ON BIOSYS.JSON_LIST TO PUBLIC;

GRANT EXECUTE ON BIOSYS.JSON_VALUE TO PUBLIC;
DROP TYPE BIOSYS.JSON;

CREATE OR REPLACE TYPE BIOSYS.json as object (
  /*
  Copyright (c) 2010 Jonas Krogsboell

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
  */

  /* Variables */
  json_data json_value_array,
  check_for_duplicate number,

  /* Constructors */
  constructor function json return self as result,
  constructor function json(str varchar2) return self as result,
  constructor function json(str in clob) return self as result,
  constructor function json(cast json_value) return self as result,
  constructor function json(l in out nocopy json_list) return self as result,

  /* Member setter methods */
  member procedure remove(pair_name varchar2),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json_value, position pls_integer default null),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value varchar2, position pls_integer default null),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value number, position pls_integer default null),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value boolean, position pls_integer default null),
  member procedure check_duplicate(self in out nocopy json, set boolean),
  member procedure remove_duplicates(self in out nocopy json),

  /* deprecated putter use json_value */
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json, position pls_integer default null),
  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json_list, position pls_integer default null),

  /* Member getter methods */
  member function count return number,
  member function get(pair_name varchar2) return json_value,
  member function get(position pls_integer) return json_value,
  member function index_of(pair_name varchar2) return number,
  member function exist(pair_name varchar2) return boolean,

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2,
  member procedure to_clob(self in json, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0),
  member procedure print(self in json, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null), --32512 is maximum
  member procedure htp(self in json, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null),

  member function to_json_value return json_value,
  /* json path */
  member function path(json_path varchar2) return json_value,

  /* json path_put */
  member procedure path_put(self in out nocopy json, json_path varchar2, elem json_value),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem varchar2),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem number),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem boolean),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem json_list),
  member procedure path_put(self in out nocopy json, json_path varchar2, elem json),

  /* map functions */
  member function get_values return json_list,
  member function get_keys return json_list

) not final;
/


DROP TYPE BIOSYS.JSON_LIST;

CREATE OR REPLACE TYPE BIOSYS.json_list as object (
  /*
  Copyright (c) 2010 Jonas Krogsboell

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
  */

  list_data json_value_array,
  constructor function json_list return self as result,
  constructor function json_list(str varchar2) return self as result,
  constructor function json_list(str clob) return self as result,
  constructor function json_list(cast json_value) return self as result,

  member procedure add_elem(self in out nocopy json_list, elem json_value, position pls_integer default null),
  member procedure add_elem(self in out nocopy json_list, elem varchar2, position pls_integer default null),
  member procedure add_elem(self in out nocopy json_list, elem number, position pls_integer default null),
  member procedure add_elem(self in out nocopy json_list, elem boolean, position pls_integer default null),

  /* deprecated putter use json_value */
  member procedure add_elem(self in out nocopy json_list, elem json_list, position pls_integer default null),

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem json_value),
  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem varchar2),
  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem number),
  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem boolean),
  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem json_list),

  member function count return number,
  member procedure remove_elem(self in out nocopy json_list, position pls_integer),
  member procedure remove_first(self in out nocopy json_list),
  member procedure remove_last(self in out nocopy json_list),
  member function get_elem(position pls_integer) return json_value,
  member function get_first return json_value,
  member function get_last return json_value,

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2,
  member procedure to_clob(self in json_list, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0),
  member procedure print(self in json_list, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null), --32512 is maximum
  member procedure htp(self in json_list, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null),

  /* json path */
  member function path(json_path varchar2) return json_value,
  /* json path_put */
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem json_value),
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem varchar2),
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem number),
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem boolean),
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem json_list),

  member function to_json_value return json_value
) not final;
/


DROP TYPE BIOSYS.JSON_VALUE;

CREATE OR REPLACE TYPE BIOSYS.json_value as object
(
  /*
  Copyright (c) 2010 Jonas Krogsboell

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
  */

  typeval number(1), /* 1 = object, 2 = array, 3 = string, 4 = number, 5 = bool, 6 = null */
  str varchar2(4000),
  num number, /* store 1 as true, 0 as false */
  object_or_array anydata, /* object or array in here */

  /* mapping */
  mapname varchar2(4000),
  mapindx number(32),

  constructor function json_value(object_or_array anydata) return self as result,
  constructor function json_value(str varchar2, esc boolean default true) return self as result,
  constructor function json_value(num number) return self as result,
  constructor function json_value(b boolean) return self as result,
  constructor function json_value return self as result,
  static function makenull return json_value,

  member function get_type return varchar2,
  member function get_string return varchar2,
  member function get_number return number,
  member function get_bool return boolean,
  member function get_null return varchar2,

  member function is_object return boolean,
  member function is_array return boolean,
  member function is_string return boolean,
  member function is_number return boolean,
  member function is_bool return boolean,
  member function is_null return boolean,

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2,
  member procedure to_clob(self in json_value, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0),
  member procedure print(self in json_value, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null), --32512 is maximum
  member procedure htp(self in json_value, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null),

  member function value_of(self in json_value) return varchar2

) not final;
/


DROP TYPE BODY BIOSYS.JSON;

CREATE OR REPLACE TYPE BODY BIOSYS.json as

  /* Constructors */
  constructor function json return self as result as
  begin
    self.json_data := json_value_array();
    self.check_for_duplicate := 1;
    return;
  end;

  constructor function json(str varchar2) return self as result as
  begin
    self := json_parser.parser(str);
    self.check_for_duplicate := 1;
    return;
  end;

  constructor function json(str in clob) return self as result as
  begin
    self := json_parser.parser(str);
    self.check_for_duplicate := 1;
    return;
  end;

  constructor function json(cast json_value) return self as result as
    x number;
  begin
    x := cast.object_or_array.getobject(self);
    self.check_for_duplicate := 1;
    return;
  end;

  constructor function json(l in out nocopy json_list) return self as result as
  begin
    for i in 1 .. l.list_data.count loop
      if(l.list_data(i).mapname is null or l.list_data(i).mapname like 'row%') then
      l.list_data(i).mapname := 'row'||i;
      end if;
      l.list_data(i).mapindx := i;
    end loop;

    self.json_data := l.list_data;
    self.check_for_duplicate := 1;
    return;
  end;

  /* Member setter methods */
  member procedure remove(self in out nocopy json, pair_name varchar2) as
    temp json_value;
    indx pls_integer;

    function get_member(pair_name varchar2) return json_value as
      indx pls_integer;
    begin
      indx := json_data.first;
      loop
        exit when indx is null;
        if(pair_name is null and json_data(indx).mapname is null) then return json_data(indx); end if;
        if(json_data(indx).mapname = pair_name) then return json_data(indx); end if;
        indx := json_data.next(indx);
      end loop;
      return null;
    end;
  begin
    temp := get_member(pair_name);
    if(temp is null) then return; end if;

    indx := json_data.next(temp.mapindx);
    loop
      exit when indx is null;
      json_data(indx).mapindx := indx - 1;
      json_data(indx-1) := json_data(indx);
      indx := json_data.next(indx);
    end loop;
    json_data.trim(1);
    --num_elements := num_elements - 1;
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json_value, position pls_integer default null) as
    insert_value json_value := nvl(pair_value, json_value.makenull);
    indx pls_integer; x number;
    temp json_value;
    function get_member(pair_name varchar2) return json_value as
      indx pls_integer;
    begin
      indx := json_data.first;
      loop
        exit when indx is null;
        if(pair_name is null and json_data(indx).mapname is null) then return json_data(indx); end if;
        if(json_data(indx).mapname = pair_name) then return json_data(indx); end if;
        indx := json_data.next(indx);
      end loop;
      return null;
    end;
  begin
    --dbms_output.put_line('PN '||pair_name);

--    if(pair_name is null) then
--      raise_application_error(-20102, 'JSON put-method type error: name cannot be null');
--    end if;
    insert_value.mapname := pair_name;
--    self.remove(pair_name);
    if(self.check_for_duplicate = 1) then temp := get_member(pair_name); else temp := null; end if;
    if(temp is not null) then
      insert_value.mapindx := temp.mapindx;
      json_data(temp.mapindx) := insert_value;
      return;
    elsif(position is null or position > self.count) then
      --insert at the end of the list
      --dbms_output.put_line('Test');
--      indx := self.count + 1;
      json_data.extend(1);
      json_data(json_data.count) := insert_value;
--      insert_value.mapindx := json_data.count;
      json_data(json_data.count).mapindx := json_data.count;
--      dbms_output.put_line('Test2'||insert_value.mapindx);
--      dbms_output.put_line('Test2'||insert_value.mapname);
--      insert_value.print(false);
--      self.print;
    elsif(position < 2) then
      --insert at the start of the list
      indx := json_data.last;
      json_data.extend;
      loop
        exit when indx is null;
        temp := json_data(indx);
        temp.mapindx := indx+1;
        json_data(temp.mapindx) := temp;
        indx := json_data.prior(indx);
      end loop;
      json_data(1) := insert_value;
      insert_value.mapindx := 1;
    else
      --insert somewhere in the list
      indx := json_data.last;
--      dbms_output.put_line('Test '||indx);
      json_data.extend;
--      dbms_output.put_line('Test '||indx);
      loop
--        dbms_output.put_line('Test '||indx);
        temp := json_data(indx);
        temp.mapindx := indx + 1;
        json_data(temp.mapindx) := temp;
        exit when indx = position;
        indx := json_data.prior(indx);
      end loop;
      json_data(position) := insert_value;
      json_data(position).mapindx := position;
    end if;
--    num_elements := num_elements + 1;
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value varchar2, position pls_integer default null) as
  begin
    put(pair_name, json_value(pair_value), position);
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value number, position pls_integer default null) as
  begin
    if(pair_value is null) then
      put(pair_name, json_value(), position);
    else
      put(pair_name, json_value(pair_value), position);
    end if;
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value boolean, position pls_integer default null) as
  begin
    if(pair_value is null) then
      put(pair_name, json_value(), position);
    else
      put(pair_name, json_value(pair_value), position);
    end if;
  end;

  member procedure check_duplicate(self in out nocopy json, set boolean) as
  begin
    if(set) then
      check_for_duplicate := 1;
    else
      check_for_duplicate := 0;
    end if;
  end;

  /* deprecated putters */

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json, position pls_integer default null) as
  begin
    if(pair_value is null) then
      put(pair_name, json_value(), position);
    else
      put(pair_name, pair_value.to_json_value, position);
    end if;
  end;

  member procedure put(self in out nocopy json, pair_name varchar2, pair_value json_list, position pls_integer default null) as
  begin
    if(pair_value is null) then
      put(pair_name, json_value(), position);
    else
      put(pair_name, pair_value.to_json_value, position);
    end if;
  end;

  /* Member getter methods */
  member function count return number as
  begin
    return self.json_data.count;
  end;

  member function get(pair_name varchar2) return json_value as
    indx pls_integer;
  begin
    indx := json_data.first;
    loop
      exit when indx is null;
      if(pair_name is null and json_data(indx).mapname is null) then return json_data(indx); end if;
      if(json_data(indx).mapname = pair_name) then return json_data(indx); end if;
      indx := json_data.next(indx);
    end loop;
    return null;
  end;

  member function get(position pls_integer) return json_value as
  begin
    if(self.count >= position and position > 0) then
      return self.json_data(position);
    end if;
    return null; -- do not throw error, just return null
  end;

  member function index_of(pair_name varchar2) return number as
    indx pls_integer;
  begin
    indx := json_data.first;
    loop
      exit when indx is null;
      if(pair_name is null and json_data(indx).mapname is null) then return indx; end if;
      if(json_data(indx).mapname = pair_name) then return indx; end if;
      indx := json_data.next(indx);
    end loop;
    return -1;
  end;

  member function exist(pair_name varchar2) return boolean as
  begin
    return (self.get(pair_name) is not null);
  end;

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2 as
  begin
    if(spaces is null) then
      return json_printer.pretty_print(self, line_length => chars_per_line);
    else
      return json_printer.pretty_print(self, spaces, line_length => chars_per_line);
    end if;
  end;

  member procedure to_clob(self in json, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0) as
  begin
    if(spaces is null) then
      json_printer.pretty_print(self, false, buf, line_length => chars_per_line);
    else
      json_printer.pretty_print(self, spaces, buf, line_length => chars_per_line);
    end if;
  end;

  member procedure print(self in json, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null) as --32512 is the real maximum in sqldeveloper
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.dbms_output_clob(my_clob, json_printer.newline_char, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member procedure htp(self in json, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null) as
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.htp_output_clob(my_clob, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member function to_json_value return json_value as
  begin
    return json_value(anydata.convertobject(self));
  end;

  /* json path */
  member function path(json_path varchar2) return json_value as
  begin
    return json_ext.get_json_value(self, json_path);
  end path;

  /* json path_put */
  member procedure path_put(self in out nocopy json, json_path varchar2, elem json_value) as
  begin
    json_ext.put(self, json_path, elem);
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem varchar2) as
  begin
    json_ext.put(self, json_path, elem);
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem number) as
  begin
    if(elem is null) then
      json_ext.put(self, json_path, json_value());
    else
      json_ext.put(self, json_path, elem);
    end if;
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem boolean) as
  begin
    if(elem is null) then
      json_ext.put(self, json_path, json_value());
    else
      json_ext.put(self, json_path, elem);
    end if;
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem json_list) as
  begin
    if(elem is null) then
      json_ext.put(self, json_path, json_value());
    else
      json_ext.put(self, json_path, elem);
    end if;
  end path_put;

  member procedure path_put(self in out nocopy json, json_path varchar2, elem json) as
  begin
    if(elem is null) then
      json_ext.put(self, json_path, json_value());
    else
      json_ext.put(self, json_path, elem);
    end if;
  end path_put;

  /* Thanks to Matt Nolan */
  member function get_keys return json_list as
    keys json_list;
    indx pls_integer;
  begin
    keys := json_list();
    indx := json_data.first;
    loop
      exit when indx is null;
      keys.add_elem(json_data(indx).mapname);
      indx := json_data.next(indx);
    end loop;
    return keys;
  end;

  member function get_values return json_list as
    vals json_list := json_list();
  begin
    vals.list_data := self.json_data;
    return vals;
  end;

  member procedure remove_duplicates(self in out nocopy json) as
  begin
    json_parser.remove_duplicates(self);
  end remove_duplicates;


end;
/


DROP TYPE BODY BIOSYS.JSON_LIST;

CREATE OR REPLACE TYPE BODY BIOSYS.json_list as

  constructor function json_list return self as result as
  begin
    self.list_data := json_value_array();
    return;
  end;

  constructor function json_list(str varchar2) return self as result as
  begin
    self := json_parser.parse_list(str);
    return;
  end;

  constructor function json_list(str clob) return self as result as
  begin
    self := json_parser.parse_list(str);
    return;
  end;

  constructor function json_list(cast json_value) return self as result as
    x number;
  begin
    x := cast.object_or_array.getobject(self);
    return;
  end;


  member procedure add_elem(self in out nocopy json_list, elem json_value, position pls_integer default null) as
    indx pls_integer;
    insert_value json_value := NVL(elem, json_value);
  begin
    if(position is null or position > self.count) then --end of list
      indx := self.count + 1;
      self.list_data.extend(1);
      self.list_data(indx) := insert_value;
    elsif(position < 1) then --new first
      indx := self.count;
      self.list_data.extend(1);
      for x in reverse 1 .. indx loop
        self.list_data(x+1) := self.list_data(x);
      end loop;
      self.list_data(1) := insert_value;
    else
      indx := self.count;
      self.list_data.extend(1);
      for x in reverse position .. indx loop
        self.list_data(x+1) := self.list_data(x);
      end loop;
      self.list_data(position) := insert_value;
    end if;

  end;

  member procedure add_elem(self in out nocopy json_list, elem varchar2, position pls_integer default null) as
  begin
    add_elem(json_value(elem), position);
  end;

  member procedure add_elem(self in out nocopy json_list, elem number, position pls_integer default null) as
  begin
    if(elem is null) then
      add_elem(json_value(), position);
    else
      add_elem(json_value(elem), position);
    end if;
  end;

  member procedure add_elem(self in out nocopy json_list, elem boolean, position pls_integer default null) as
  begin
    if(elem is null) then
      add_elem(json_value(), position);
    else
      add_elem(json_value(elem), position);
    end if;
  end;

  member procedure add_elem(self in out nocopy json_list, elem json_list, position pls_integer default null) as
  begin
    if(elem is null) then
      add_elem(json_value(), position);
    else
      add_elem(elem.to_json_value, position);
    end if;
  end;

 member procedure set_elem(self in out nocopy json_list, position pls_integer, elem json_value) as
    insert_value json_value := NVL(elem, json_value);
    indx number;
  begin
    if(position > self.count) then --end of list
      indx := self.count + 1;
      self.list_data.extend(1);
      self.list_data(indx) := insert_value;
    elsif(position < 1) then --maybe an error message here
      null;
    else
      self.list_data(position) := insert_value;
    end if;
  end;

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem varchar2) as
  begin
    set_elem(position, json_value(elem));
  end;

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem number) as
  begin
    if(elem is null) then
      set_elem(position, json_value());
    else
      set_elem(position, json_value(elem));
    end if;
  end;

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem boolean) as
  begin
    if(elem is null) then
      set_elem(position, json_value());
    else
      set_elem(position, json_value(elem));
    end if;
  end;

  member procedure set_elem(self in out nocopy json_list, position pls_integer, elem json_list) as
  begin
    if(elem is null) then
      set_elem(position, json_value());
    else
      set_elem(position, elem.to_json_value);
    end if;
  end;

  member function count return number as
  begin
    return self.list_data.count;
  end;

  member procedure remove_elem(self in out nocopy json_list, position pls_integer) as
  begin
    if(position is null or position < 1 or position > self.count) then return; end if;
    for x in (position+1) .. self.count loop
      self.list_data(x-1) := self.list_data(x);
    end loop;
    self.list_data.trim(1);
  end;

  member procedure remove_first(self in out nocopy json_list) as
  begin
    for x in 2 .. self.count loop
      self.list_data(x-1) := self.list_data(x);
    end loop;
    if(self.count > 0) then
      self.list_data.trim(1);
    end if;
  end;

  member procedure remove_last(self in out nocopy json_list) as
  begin
    if(self.count > 0) then
      self.list_data.trim(1);
    end if;
  end;

  member function get_elem(position pls_integer) return json_value as
  begin
    if(self.count >= position and position > 0) then
      return self.list_data(position);
    end if;
    return null; -- do not throw error, just return null
  end;

  member function get_first return json_value as
  begin
    if(self.count > 0) then
      return self.list_data(self.list_data.first);
    end if;
    return null; -- do not throw error, just return null
  end;

  member function get_last return json_value as
  begin
    if(self.count > 0) then
      return self.list_data(self.list_data.last);
    end if;
    return null; -- do not throw error, just return null
  end;

  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2 as
  begin
    if(spaces is null) then
      return json_printer.pretty_print_list(self, line_length => chars_per_line);
    else
      return json_printer.pretty_print_list(self, spaces, line_length => chars_per_line);
    end if;
  end;

  member procedure to_clob(self in json_list, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0) as
  begin
    if(spaces is null) then
      json_printer.pretty_print_list(self, false, buf, line_length => chars_per_line);
    else
      json_printer.pretty_print_list(self, spaces, buf, line_length => chars_per_line);
    end if;
  end;

  member procedure print(self in json_list, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null) as --32512 is the real maximum in sqldeveloper
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print_list(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.dbms_output_clob(my_clob, json_printer.newline_char, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member procedure htp(self in json_list, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null) as
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print_list(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.htp_output_clob(my_clob, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  /* json path */
  member function path(json_path varchar2) return json_value as
    cp json_list := self;
  begin
    return json_ext.get_json_value(json(cp), json_path);
  end path;


  /* json path_put */
  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem json_value) as
    objlist json := json(self);
  begin
    json_ext.put(objlist, json_path, elem);
    self := objlist.get_values;
  end path_put;

  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem varchar2) as
    objlist json := json(self);
  begin
    json_ext.put(objlist, json_path, elem);
    self := objlist.get_values;
  end path_put;

  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem number) as
    objlist json := json(self);
  begin
    if(elem is null) then
      json_ext.put(objlist, json_path, json_value);
    else
      json_ext.put(objlist, json_path, elem);
    end if;
    self := objlist.get_values;
  end path_put;

  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem boolean) as
    objlist json := json(self);
  begin
    if(elem is null) then
      json_ext.put(objlist, json_path, json_value);
    else
      json_ext.put(objlist, json_path, elem);
    end if;
    self := objlist.get_values;
  end path_put;

  member procedure path_put(self in out nocopy json_list, json_path varchar2, elem json_list) as
    objlist json := json(self);
  begin
    if(elem is null) then
      json_ext.put(objlist, json_path, json_value);
    else
      json_ext.put(objlist, json_path, elem);
    end if;
    self := objlist.get_values;
  end path_put;

  member function to_json_value return json_value as
  begin
    return json_value(anydata.convertobject(self));
  end;

end;
/


DROP TYPE BODY BIOSYS.JSON_VALUE;

CREATE OR REPLACE TYPE BODY BIOSYS.json_value as

  constructor function json_value(object_or_array anydata) return self as result as
  begin
    case object_or_array.gettypename
      when sys_context('userenv', 'current_schema')||'.JSON_LIST' then self.typeval := 2;
      when sys_context('userenv', 'current_schema')||'.JSON' then self.typeval := 1;
      else raise_application_error(-20102, 'JSON_Value init error (JSON or JSON\_List allowed)');
    end case;
    self.object_or_array := object_or_array;
    if(self.object_or_array is null) then self.typeval := 6; end if;

    return;
  end json_value;

  constructor function json_value(str varchar2, esc boolean default true) return self as result as
  begin
    self.typeval := 3;
    if(esc) then self.num := 1; else self.num := 0; end if; --message to pretty printer
    self.str := str;
    return;
  end json_value;

  constructor function json_value(num number) return self as result as
  begin
    self.typeval := 4;
    self.num := num;
    if(self.num is null) then self.typeval := 6; end if;
    return;
  end json_value;

  constructor function json_value(b boolean) return self as result as
  begin
    self.typeval := 5;
    self.num := 0;
    if(b) then self.num := 1; end if;
    if(b is null) then self.typeval := 6; end if;
    return;
  end json_value;

  constructor function json_value return self as result as
  begin
    self.typeval := 6; /* for JSON null */
    return;
  end json_value;

  static function makenull return json_value as
  begin
    return json_value;
  end makenull;

  member function get_type return varchar2 as
  begin
    case self.typeval
    when 1 then return 'object';
    when 2 then return 'array';
    when 3 then return 'string';
    when 4 then return 'number';
    when 5 then return 'bool';
    when 6 then return 'null';
    end case;

    return 'unknown type';
  end get_type;

  member function get_string return varchar2 as
  begin
    if(self.typeval = 3) then
      return self.str;
    end if;
    return null;
  end get_string;

  member function get_number return number as
  begin
    if(self.typeval = 4) then
      return self.num;
    end if;
    return null;
  end get_number;

  member function get_bool return boolean as
  begin
    if(self.typeval = 5) then
      return self.num = 1;
    end if;
    return null;
  end get_bool;

  member function get_null return varchar2 as
  begin
    if(self.typeval = 6) then
      return 'null';
    end if;
    return null;
  end get_null;

  member function is_object return boolean as begin return self.typeval = 1; end;
  member function is_array return boolean as begin return self.typeval = 2; end;
  member function is_string return boolean as begin return self.typeval = 3; end;
  member function is_number return boolean as begin return self.typeval = 4; end;
  member function is_bool return boolean as begin return self.typeval = 5; end;
  member function is_null return boolean as begin return self.typeval = 6; end;

  /* Output methods */
  member function to_char(spaces boolean default true, chars_per_line number default 0) return varchar2 as
  begin
    if(spaces is null) then
      return json_printer.pretty_print_any(self, line_length => chars_per_line);
    else
      return json_printer.pretty_print_any(self, spaces, line_length => chars_per_line);
    end if;
  end;

  member procedure to_clob(self in json_value, buf in out nocopy clob, spaces boolean default false, chars_per_line number default 0) as
  begin
    if(spaces is null) then
      json_printer.pretty_print_any(self, false, buf, line_length => chars_per_line);
    else
      json_printer.pretty_print_any(self, spaces, buf, line_length => chars_per_line);
    end if;
  end;

  member procedure print(self in json_value, spaces boolean default true, chars_per_line number default 8192, jsonp varchar2 default null) as --32512 is the real maximum in sqldeveloper
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print_any(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.dbms_output_clob(my_clob, json_printer.newline_char, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member procedure htp(self in json_value, spaces boolean default false, chars_per_line number default 0, jsonp varchar2 default null) as
    my_clob clob;
  begin
    my_clob := empty_clob();
    dbms_lob.createtemporary(my_clob, true);
    json_printer.pretty_print_any(self, spaces, my_clob, case when (chars_per_line>32512) then 32512 else chars_per_line end);
    json_printer.htp_output_clob(my_clob, jsonp);
    dbms_lob.freetemporary(my_clob);
  end;

  member function value_of(self in json_value) return varchar2 as
  begin
    case self.typeval
    when 1 then return 'json object';
    when 2 then return 'json array';
    when 3 then return self.get_string();
    when 4 then return self.get_number();
    when 5 then if(self.get_bool()) then return 'true'; else return 'false'; end if;
    else return null;
    end case;
  end;


end;
/


GRANT EXECUTE ON BIOSYS.JSON TO PUBLIC;

GRANT EXECUTE ON BIOSYS.JSON_LIST TO PUBLIC;

GRANT EXECUTE ON BIOSYS.JSON_VALUE TO PUBLIC;
DROP SEQUENCE BIOSYS.SEQ_USRIN_LOG;

CREATE SEQUENCE BIOSYS.SEQ_USRIN_LOG
  START WITH 85945
  MAXVALUE 999999999999999999999999999
  MINVALUE 0
  NOCYCLE
  CACHE 2
  NOORDER;
