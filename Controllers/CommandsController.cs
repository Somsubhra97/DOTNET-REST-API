using System.Collections.Generic;
using AutoMapper;
using Commander.Data;
using Commander.Dtos;
using Commander.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace Commander.Controllers
{

    [Route("api/commands")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        //private readonly MockCommander repo=new MockCommander();

        private readonly ICommanderRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommanderRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
       
        //GET api/commands
        [HttpGet]
        public ActionResult <IEnumerable<CommandReadDto>> GetAllCommmands()
        {

            //With dep injection
            //return Ok(_reposiory.GetAllCommands());

            var commandItems = _repository.GetAllCommands();

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        //GET api/commands/{id}
        [HttpGet("{id}", Name="GetCommandById")]
        public ActionResult <CommandReadDto> GetCommandById(int id)
        {

            //With dep injection
            //return Ok(_reposiory.GetCommandById(id));
            var commandItem = _repository.GetCommandById(id);
            if(commandItem == null)            
                return NotFound();                
            
            return Ok(_mapper.Map<CommandReadDto>(commandItem));
            
        }

        //POST api/commands
        [HttpPost]
        public ActionResult <CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto)
        {
            //Accept DTO Obj, cnvrt to Command model, pass it down to SQL Commander to add data return DTO 
            var commandModel = _mapper.Map<Command>(commandCreateDto);
            _repository.CreateCommand(commandModel);
            _repository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(commandModel);

            return CreatedAtRoute(nameof(GetCommandById), new {Id = commandReadDto.Id}, commandReadDto);      
        }

        //PUT api/commands/{id}
        [HttpPut("{id}")]
        public ActionResult UpdateCommand(int id, CommandUpdateDto commandUpdateDto)
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if(commandModelFromRepo == null)
            {
                return NotFound();
            }
            _mapper.Map(commandUpdateDto, commandModelFromRepo);

            _repository.UpdateCommand(commandModelFromRepo);

            _repository.SaveChanges();

            return NoContent();
        }

        /*
            [{
                "op":"replace", //add,remove,replace,copymove,test for patch actions
                "path":"howto", //model field
                "value:"changer" //value re-written on model prop
            }]
        */
        //PATCH api/commands/{id}
        [HttpPatch("{id}")]
        public ActionResult PartialCommandUpdate(int id, JsonPatchDocument<CommandUpdateDto> patchDoc)
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if(commandModelFromRepo == null)
            {
                return NotFound();
            }

            var commandToPatch = _mapper.Map<CommandUpdateDto>(commandModelFromRepo);
            patchDoc.ApplyTo(commandToPatch, ModelState);

            if(!TryValidateModel(commandToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(commandToPatch, commandModelFromRepo);

            _repository.UpdateCommand(commandModelFromRepo);

            _repository.SaveChanges();

            return NoContent();
        }

        //DELETE api/commands/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteCommand(int id)
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if(commandModelFromRepo == null)
            {
                return NotFound();
            }
            _repository.DeleteCommand(commandModelFromRepo);
            _repository.SaveChanges();

            return NoContent();
        }
        
    }
}