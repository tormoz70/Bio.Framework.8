CREATE OR REPLACE PACKAGE ddl$utl
  IS
  procedure reset_seq(seq_name in varchar2, tbl_name in varchar2, id_fld_name in varchar2);
    function table_exists(p_table_name in varchar2) return number;

END; -- Package spec
/
CREATE OR REPLACE PACKAGE BODY ddl$utl
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
    ai$utl.pars_tablename(tbl_name,v_schema_name,v_table_name);
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

  function table_exists(p_table_name in varchar2) return number
  is
    vCnt pls_integer;
    v_schema_name varchar2(32);
    v_table_name varchar2(32);
  begin
    ai$utl.pars_tablename(p_table_name,v_schema_name,v_table_name);
    select count(1) into vCnt from all_tables a
     where a.owner = upper(v_schema_name)
       and a.table_name = upper(v_table_name);
    return (case when vCnt > 0 then 1 else 0 end);
  end;

END;
/
