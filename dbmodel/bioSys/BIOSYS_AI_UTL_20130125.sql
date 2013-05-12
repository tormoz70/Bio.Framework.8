-- Start of DDL Script for Package BIOSYS.AI_UTL
-- Generated 25.01.2013 15:15:07 from BIOSYS@GIVCDB_EKBS02

CREATE OR REPLACE 
PACKAGE biosys.ai_utl
IS
  type T_VARCHAR_TBL is table of varchar2 (4000);

  type T_INLIST_REC is record (item   varchar2 (4000));

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
  function get_word (pSource varchar2, pIndex number, pDelimeter varchar2 := '/') return varchar2;
  procedure sleep (pInterval in number);
  function sleep (pInterval in number) return number;
  procedure pars_tablename(full_tbl_name in varchar2, schm_name out varchar2, tbl_name out varchar2);
  function split_str(pStr in varchar2, pDelimeter in varchar2 default ',') return T_VARCHAR_TBL pipelined;
  function lists_is_equals(p_lst1 in varchar2, p_lst2 in varchar2, p_delimeter in varchar2) return number;
  procedure compare_lists(
    p_lst1 in varchar2, p_lst2 in varchar2, p_delimeter in varchar2,
    v_add_to_lst1 out varchar2, v_add_to_lst2 out varchar2);
  function pwd (nlength in number) return varchar2;
  function decrypt_string(input_string in varchar2, key_line in varchar2) return varchar2;
  function encrypt_string(input_string in varchar2, key_line in varchar2) return varchar2;
  function invert_list(pStr in varchar2, pDelimeter in varchar2 default ',') return varchar2;
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

  function day_of_week(p_date in date) return number;

  function nextday(p_inday date, p_weekday varchar2) return date;

  procedure time_between(pDtStart in date, pDtEnd in date,
    vDayDure out number, vHHDure out number, vMIDure out number, vSSDure out number);

  function time_between_secs(pDtStart in date, pDtEnd in date) return number;
  function db_datetime(p_date date, p_remote_time_zone number) return date;

END;
/

-- Grants for Package
GRANT EXECUTE ON biosys.ai_utl TO public
/
GRANT EXECUTE ON biosys.ai_utl TO givc_org
/

CREATE OR REPLACE 
PACKAGE BODY biosys.ai_utl
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
    addLastEmptyItem boolean := false;
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
          addLastEmptyItem := vLine is null;
        else
          vLine2Add:=vLine;
          vLine:='';
        end if;
        --dbms_output.put_line('vLine:'||nvl(vLine, 'NULL'));
        pipe row (vLine2Add);
      end loop;
      if addLastEmptyItem then
        pipe row ('');
      end if;
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
    vDayDure number(10);
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

  function day_of_week(p_date in date) return number
  is
    v_day varchar2(100);
  begin
    SELECT TO_CHAR (p_date, 'DAY') into v_day FROM DUAL;
    v_day := trim(upper(v_day));
    return case when v_day = 'MONDAY' then 1
                when v_day = 'TUESDAY' then 2
                when v_day = 'WEDNESDAY' then 3
                when v_day = 'THURSDAY' then 4
                when v_day = 'FRIDAY' then 5
                when v_day = 'SATURDAY' then 6
                when v_day = 'SUNDAY' then 7
                else 0 end;
  end;

  function nextday(p_inday date, p_weekday varchar2) return date
  is
    type t_weekdays is varray(7) of varchar2(32);
    v_weekdays t_weekdays := t_weekdays('MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY', 'SUNDAY');
    v_day date;
    v_indx pls_integer;
    v_inday_indx pls_integer;
  begin
    for i in 1..7 loop
      if upper(p_weekday) = v_weekdays(i) then
        v_indx := i;
        exit;
      end if;
    end loop;
    --dbms_output.put_line('v_indx: '||v_indx);
    v_inday_indx := day_of_week(p_inday);
    --dbms_output.put_line('v_inday_indx: '||v_inday_indx);
    if v_inday_indx >= v_indx then
      v_indx := 7 - v_inday_indx + v_indx;
    else
      v_indx := v_indx - v_inday_indx;
    end if;
    --dbms_output.put_line('v_indx1: '||v_indx);
    v_day := p_inday + v_indx;
    --dbms_output.put_line('p_inday:'||p_inday||'; v_day: '||v_day);
    return v_day;
  end;

  function db_datetime(p_date date, p_remote_time_zone number) return date
  is
    v_db_time_zone number(4) := to_number(to_char(current_timestamp, 'TZH'));
  begin
    return p_date + (nvl(p_remote_time_zone, v_db_time_zone)-v_db_time_zone)/1440*60;
  end;
END;
/



