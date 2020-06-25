# Grupo 1 - Agenda de turnos

### Integrantes
> - Capelli, Ricardo Armando	<ricki.capelli@gmail.com>
> - Krug, Alan Facundo	<akrug@ort.edu.ar>
> - Muñoz, Santiago Marcos Luis	<santiagomlmunoz@gmail.com>
> - Nevado, Santiago Ariel	<nevadosantiago97@gmail.com>

### Idea
La siguiente agenda será modelada para un centro odontológico. 
Este consultorio es muy flexible con respecto al uso de consultorios, profesionales y duración de las prestaciones. 
 
### Modelos
* Paciente
* Profesional 
* Turno 
* Consultorio 
 
### Consideraciones mínimas
 - Un paciente tiene  
   - Una dirección 
   - Un turno asignado 
 
 - Un turno tiene: 
   - Un paciente dueño 
   - Un profesional encargado 
   - Un consultorio asignado 
 
 - Un profesional tiene: 
   - Horarios y días de atención 
   - Turnos asignados 
   - Una prestación. Ej. solo hace tratamientos de conducto (cada prestación tiene una duración fija) 
 
 - El consultorio tiene: 
   - Horarios de funcionamiento 
   - Turnos asignados. 
 
##### Se requiere que 
 - Se puedan cargar y administrar   
   - Profesionales 
   - Pacientes 
   - Consultorio  
 - Los pacientes puedan sacar un turno. 
   - Puede buscar turnos por prestación 
   - Los turnos disponibles se indicarán por fecha y por profesional, solo si hay consultorios disponibles.
   - El turno tiene que ser en el futuro 
   - Solo puede tener uno. 
   - Si quiere puede cancelarlo antes de la fecha. 
 - Los profesionales puede listar los turnos realizados en el mes calendario y le indican la remuneración que recibirán en base al valor que tienen descrito para la prestación.
 - Los turnos pueden ser confirmados que se tomaron (comienza la prestación) dentro de una ventana de +-15 minutos de la hora de inicio. 
 - No se pueden solapar turnos/profesionales/consultorios 
 - Se tiene que poder visualizar la disponibilidad por consultorio 
