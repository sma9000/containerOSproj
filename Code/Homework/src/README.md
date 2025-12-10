Part 1 – Running with Docker Compose

Step 1. Start Docker Compose
docker compose up -d
What this does:
Starts all the services defined in the docker-compose.yml file in the background (-d = detached). Each container (SQL, RabbitMQ, Web Status, Web MVC, Web SPA, etc.) is created and networked together automatically.

Step 2. Verify services are running
docker ps
What this does:
Lists all running containers. You should see services like:

sqldata (SQL Server)

rabbitmq (RabbitMQ with management console)

webstatus, webmvc, webspa

Step 3. Test service reachability
Open these URLs in your browser:

Web Status: http://localhost:5107/

Web MVC: http://localhost:5100/

Web SPA: http://localhost:5104/
What this does:
Confirms that your containers are accessible from the host. Each service has its own published port that maps container → host.

Step 4. Stop containers
docker compose down
What this does:
Stops and removes all containers created by the docker-compose.yml. This resets your environment before moving on to Swarm.


Part 2 – Running with Docker Swarm
Step 1. Initialize Docker Swarm
docker swarm init
What this does:
Turns your Docker engine into a Swarm manager. This is required before deploying a stack.

Step 2. Generate the stack file
docker compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml config > stack.rendered.yml
What this does:
Merges docker-compose.yml, overrides, and .env into a single resolved file stack.rendered.yml.
This file is Swarm-ready.

Step 3. Validate the stack file
docker stack config -c stack.rendered.yml
What this does:
Validates that your stack.rendered.yml file is formatted correctly for Swarm.
If no errors appear, the file is ready.

Step 4. Deploy the stack
docker stack deploy -c stack.rendered.yml mystack
What this does:
Deploys all the services in your stack.rendered.yml under the stack name mystack.
Unlike Compose, Swarm manages containers as “services” with replicas, networking, and scaling.

Step 5. Check running services
docker stack services mystack
What this does:
Shows all the services running in your stack (replicas, ports, and status).

Step 6. Test reachability again

Open these URLs in your browser (now running under Swarm):

Web Status: http://localhost:5107/

Web MVC: http://localhost:5100/

Web SPA: http://localhost:5104/

Step 7. Tear down the stack
docker stack rm mystack
What this does:
Removes all services and networks deployed with the stack.

docker swarm leave --force
What this does:
Makes your Docker engine leave the swarm. This fully resets the environment.