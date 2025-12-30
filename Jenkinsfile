pipeline {
    	
	agent any

	environment {
		PROJECT           = './Booth.DockerVolumeBackup.WebApi/Booth.DockerVolumeBackup.WebApi.csproj'
		TEST_PROJECT      = './Booth.DockerVolumeBackup.Test/Booth.DockerVolumeBackup.Test.csproj'
		PORTAINER_WEBHOOK = credentials('dockervolumebackup_webhook')
    }

    stages {
					
		stage('Build') {
			agent { 
				dockerfile { 
					filename 'JenkinsBuildAgentDockerFile' 
					reuseNode true
				}
			}

			stages {
				stage('Build') {
					steps {
						sh "dotnet build ${PROJECT} --configuration Release"
					}
				}

				stage('Test') {
					steps {
						sh "rm -rf ./testresults"
						sh "dotnet test ${TEST_PROJECT} --configuration Release --logger trx  --collect \"XPlat Code Coverage\" --results-directory ./testresults"
					}
					post {
						always {
							xunit (
								thresholds: [ skipped(failureThreshold: '0'), failed(failureThreshold: '0') ],
								tools: [ MSTest(pattern: 'testresults/*.trx') ]
								)

							recordCoverage(tools: [[parser: 'COBERTURA', pattern: '**/*.xml']], sourceDirectories: [[path: './testresults']])
						}
					}
				}

				stage('Publish') {
					steps {
						sh "dotnet publish ${PROJECT} --configuration Release --output ./deploy"
					}
				}
			}

		}
		
		stage('Create Image') {
			steps {
				script {
					def dockerImage = docker.build("craigbooth/dockervolumebackup")		
					docker.withRegistry(credentialsId: 'DockerHub') {
						dockerImage.push()
					}
				}
            }
		}

		stage('Deploy') {
			steps {
				httpRequest httpMode: 'POST', responseHandle: 'NONE', url: "${PORTAINER_WEBHOOK}", wrapAsMultipart: false
            }
		}
    }
	
	post {
		success {
			cleanWs()
		}
	}
}
