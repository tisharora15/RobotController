
// ============================================================
//  Robot Controller API — Jenkins Pipeline
//  All 7 stages: Build, Test, Code Quality, Security,
//                Deploy (staging), Release (production),
//                Monitoring & Alerting
// ============================================================

pipeline {
    agent any

    // ── Environment variables ────────────────────────────────────────────
    environment {
        APP_NAME        = "robot-controller-api"
        IMAGE_NAME      = "robot_controller_api"
        IMAGE_TAG       = "${BUILD_NUMBER}"
        DOCKER_REGISTRY = "localhost:5000"

        // Credentials stored in Jenkins Credentials store
        SONAR_TOKEN     = credentials("sonar-token")
        SNYK_TOKEN      = credentials("snyk-token")
        PROD_DB_CONN    = credentials("prod-db-connection-string")
    }

    // ── Triggers ─────────────────────────────────────────────────────────
    triggers {
        pollSCM("H/5 * * * *")
    }

    // ── Global options ────────────────────────────────────────────────────
    options {
        buildDiscarder(logRotator(numToKeepStr: "10"))
        timestamps()
        timeout(time: 30, unit: "MINUTES")
    }

    stages {

        // ── STAGE 1: CHECKOUT ────────────────────────────────────────────
        stage("Checkout") {
            steps {
                echo "── Checking out source code ──"
                checkout scm
                script {
                    env.GIT_COMMIT_SHORT = sh(
                        script: "git rev-parse --short HEAD",
                        returnStdout: true
                    ).trim()
                    echo "Commit: ${env.GIT_COMMIT_SHORT}"
                }
            }
        }

        // ── STAGE 2: BUILD ───────────────────────────────────────────────
        stage("Build") {
            steps {
                echo "── Building .NET application ──"
                sh "dotnet restore robot-controller-api.csproj"
                sh "dotnet build robot-controller-api.csproj -c Release --no-restore"
                sh "dotnet publish robot-controller-api.csproj -c Release -o ./publish --no-restore"

                echo "── Building Docker image ──"
                sh """
                    docker build \
                        --target runtime \
                        -t ${IMAGE_NAME}:${IMAGE_TAG} \
                        -t ${IMAGE_NAME}:latest \
                        .
                """
            }
            post {
                success {
                    archiveArtifacts artifacts: "publish/**", fingerprint: true
                    echo "Build artefact archived."
                }
            }
        }

        // ── STAGE 3: TEST ────────────────────────────────────────────────
        stage("Test") {
            steps {
                echo "── Running unit and integration tests ──"
                sh """
                    dotnet test tests/robot_controller_api.Tests.csproj \
                        --no-restore \
                        --logger "trx;LogFileName=test-results.xml" \
                        --collect:"XPlat Code Coverage" \
                        --results-directory ./TestResults
                """
            }
            post {
                always {
                    junit "TestResults/**/*.xml"
                    publishCoverage adapters: [
                        coberturaAdapter("TestResults/**/coverage.cobertura.xml")
                    ]
                }
            }
        }

        // ── STAGE 4: CODE QUALITY ────────────────────────────────────────
        stage("Code Quality") {
            steps {
                echo "── Running SonarQube analysis ──"
                withSonarQubeEnv("SonarQube") {
                    sh """
                        dotnet sonarscanner begin \
                            /k:"${APP_NAME}" \
                            /d:sonar.login="${SONAR_TOKEN}" \
                            /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml" \
                            /d:sonar.coverage.exclusions="**/Migrations/**,**/Program.cs" \
                            /d:sonar.exclusions="**/obj/**,**/bin/**"

                        dotnet build robot-controller-api.csproj -c Release --no-restore

                        dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"
                    """
                }
            }
            post {
                always {
                    script {
                        def qualityGate = waitForQualityGate()
                        if (qualityGate.status != "OK") {
                            error "SonarQube Quality Gate FAILED: ${qualityGate.status}"
                        }
                    }
                }
            }
        }

        // ── STAGE 5: SECURITY SCAN ───────────────────────────────────────
        stage("Security") {
            parallel {

                // Snyk: dependency vulnerability scan
                stage("Snyk Dependency Scan") {
                    steps {
                        echo "── Snyk dependency scan ──"
                        sh """
                            snyk auth ${SNYK_TOKEN}
                            snyk test \
                                --file=robot-controller-api.csproj \
                                --severity-threshold=high \
                                --json > snyk-results.json || true
                        """
                    }
                    post {
                        always {
                            archiveArtifacts artifacts: "snyk-results.json", allowEmptyArchive: true
                        }
                    }
                }

                // Trivy: Docker image vulnerability scan
                stage("Trivy Image Scan") {
                    steps {
                        echo "── Trivy image scan ──"
                        sh """
                            trivy image \
                                --exit-code 0 \
                                --severity HIGH,CRITICAL \
                                --format json \
                                --output trivy-results.json \
                                ${IMAGE_NAME}:${IMAGE_TAG}

                            trivy image \
                                --exit-code 0 \
                                --severity HIGH,CRITICAL \
                                ${IMAGE_NAME}:${IMAGE_TAG}
                        """
                    }
                    post {
                        always {
                            archiveArtifacts artifacts: "trivy-results.json", allowEmptyArchive: true
                        }
                    }
                }
            }
        }

        // ── STAGE 6: DEPLOY (STAGING) ─────────────────────────────────────
        stage("Deploy") {
            steps {
                echo "── Deploying to staging environment ──"
                sh """
                    docker tag ${IMAGE_NAME}:${IMAGE_TAG} ${IMAGE_NAME}:staging

                    docker compose \
                        -f docker-compose.yml \
                        --project-name staging \
                        down --remove-orphans || true

                    docker compose \
                        -f docker-compose.yml \
                        --project-name staging \
                        up -d --build
                """

                sh """
                    echo "Waiting for staging API health check..."
                    for i in \$(seq 1 12); do
                        STATUS=\$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health/live || echo "000")
                        if [ "\$STATUS" = "200" ]; then
                            echo "Staging API is healthy after \$i attempts."
                            exit 0
                        fi
                        echo "Attempt \$i: status \$STATUS — retrying in 5s..."
                        sleep 5
                    done
                    echo "Staging health check failed after 60s."
                    exit 1
                """
            }
        }

        // ── STAGE 7: RELEASE (PRODUCTION) ────────────────────────────────
        stage("Release") {
            when {
                branch "main"
            }
            steps {
                echo "── Promoting to production ──"

                sh """
                    docker tag ${IMAGE_NAME}:${IMAGE_TAG} ${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}
                    docker tag ${IMAGE_NAME}:${IMAGE_TAG} ${DOCKER_REGISTRY}/${IMAGE_NAME}:latest
                    docker push ${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}
                    docker push ${DOCKER_REGISTRY}/${IMAGE_NAME}:latest
                """

                sh """
                    git tag -a v${IMAGE_TAG} -m "Release v${IMAGE_TAG} (Jenkins build #${BUILD_NUMBER})"
                    git push origin v${IMAGE_TAG} || true
                """

                sh """
                    export IMAGE_TAG=${IMAGE_TAG}
                    export PROD_DB_CONNECTION_STRING=${PROD_DB_CONN}

                    docker compose \
                        -f docker-compose.yml \
                        -f docker-compose.prod.yml \
                        --project-name production \
                        up -d --no-build
                """

                sh """
                    echo "Verifying production health..."
                    for i in \$(seq 1 12); do
                        STATUS=\$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health || echo "000")
                        if [ "\$STATUS" = "200" ]; then
                            echo "Production is healthy."
                            exit 0
                        fi
                        sleep 5
                    done
                    echo "Production health check failed — rolling back."
                    docker compose \
                        -f docker-compose.yml \
                        -f docker-compose.prod.yml \
                        --project-name production \
                        down
                    exit 1
                """
            }
        }

        // ── STAGE 8: MONITORING ───────────────────────────────────────────
        stage("Monitoring") {
            steps {
                echo "── Verifying Prometheus + Grafana monitoring stack ──"

                sh """
                    docker compose \
                        -f docker-compose.yml \
                        --project-name staging \
                        up -d prometheus grafana
                """

                sh """
                    sleep 10
                    PROM_STATUS=\$(curl -s "http://localhost:9090/api/v1/targets" \
                        | grep -o '"health":"up"' | wc -l)
                    echo "Prometheus healthy targets: \$PROM_STATUS"
                    if [ "\$PROM_STATUS" -lt "1" ]; then
                        echo "WARNING: Prometheus has no healthy targets."
                    fi
                """

                sh """
                    GRAFANA_STATUS=\$(curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/api/health)
                    echo "Grafana status: \$GRAFANA_STATUS"
                """

                echo "Monitoring stack is active."
                echo "Prometheus: http://localhost:9090"
                echo "Grafana:    http://localhost:3000  (admin/admin)"
                echo "API Metrics:http://localhost:8080/metrics"
            }
        }
    }

    // ── Post-pipeline actions ─────────────────────────────────────────────
    post {

        success {
            echo "Pipeline SUCCEEDED — Build #${BUILD_NUMBER} deployed successfully."
        }

        failure {
            echo "Pipeline FAILED — Build #${BUILD_NUMBER}."
        }

        always {
            echo "Cleaning up workspace..."
            cleanWs()
        }
    }
}