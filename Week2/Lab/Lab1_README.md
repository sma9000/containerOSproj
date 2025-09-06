# Lab 1 – Basic Dockerfile (API)

This lab focuses on writing a basic Dockerfile for the **Webapp** of the Example Voting App.

---

## **Objective**
Write a Dockerfile that can package and run the UI service locally.

---

## **Steps**

1. Clone the repo and navigate to the root:
   ```bash
   git clone https://github.com/msalemcode/Container_Operating_Systems.git
   cd Container_Operating_Systems/Code/Lab/voting-app
   ```

2. Navigate into the `vote` folder and create a `Dockerfile`:
Complete the following Dockerfile
   ```dockerfile
      # base defines a base stage that uses the official python runtime base image
      FROM python:3.11-slim AS base

      # Set the application directory
      
      # COPY requirements.txt
      COPY requirements.txt ./requirements.txt

      # RUN command to install requirement using pip
      RUN pip install --no-cache-dir -r requirements.txt


      # Copy our code from the current folder to the working directory inside the container



      # Make port 80 available for links and/or publish
      
      # Define our command to be run when launching the container
      ENV FLASK_ENV=development
      CMD ["python", "app.py"]

   ```


3. Build the Docker image:
   ```bash
   docker build -t voting .
   ```


4. Run  Docker image:
   ```bash
   docker run -it voting .
   ```
