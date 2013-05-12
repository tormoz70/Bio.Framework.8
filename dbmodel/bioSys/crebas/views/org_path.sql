create or replace view org_path as
select org_id, prnt_org_id, aname, adesc, replace(f_path, '|-|', '/') as f_path, replace(f_id_path, '|-|', '/') as f_id_path from(
select a.org_id, nvl(a.prnt_org_id, 'ROOT-NODE') as prnt_org_id, a.aname, a.adesc,
      sys_connect_by_path(a.aname, '|-|') as f_path,
      sys_connect_by_path(a.org_id, '|-|') as f_id_path
      from org a
     start with a.prnt_org_id is null
    connect by prior a.org_id = a.prnt_org_id)

