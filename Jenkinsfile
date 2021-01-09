pipeline {
    	
	agent any

	environment {
		PROJECT      = './Booth.DockerVolumeBackup/Booth.DockerVolumeBackup.csproj'
    }

    stages {
		stage('Build') {
			agent { 
				docker { 
					image 'mcr.microsoft.com/dotnet/sdk:5.0-alpine' 
					reuseNode true
				}
			}

			stages {
				stage('Build') {
					steps {
						sh "dotnet build ${PROJECT} --configuration Release"
					}
				}

				stage('Publish') {
					steps {
						sh "dotnet publish ${PROJECT} --configuration Release --output ./deploy"
					}
				}
			}

		}
		
		stage('Deploy') {
			steps {
				script {
					def dockerImage = docker.build("craigbooth/dockervolumebackup")
					httpRequest httpMode: 'POST', responseHandle: 'NONE', url: 'https://portainer.boothfamily.id.au/api/webhooks/f70bd8fe-e97a-4b36-ab0d-86257c4b33dc', wrapAsMultipart: false
				}
            }
		}
    }
	
	post {
		success {
			cleanWs()
		}
	}
}