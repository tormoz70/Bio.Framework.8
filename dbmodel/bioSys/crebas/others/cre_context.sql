drop context bio_context;
create context bio_context using LOGIN$UTL;

begin 
  LOGIN$UTL.set_context_value('FTW', 'QWE'); 
end;

