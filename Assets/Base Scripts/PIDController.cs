namespace Base_Scripts
{
    [System.Serializable]
    public class PIDController
    {
        public float Kp, Ki, Kd;

        private float _integralSum;
        private float _lastError;
        private bool _isFirstRun = true;

        public PIDController(float kp, float ki, float kd)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
        }

        /// <summary>
        /// Calcule l'accélération requise pour atteindre le setpoint (vitesse cible).
        /// </summary>
        public float Calculate(float setpoint, float currentValue, float deltaTime)
        {
            if (deltaTime <= 0) return 0;

            float error = setpoint - currentValue;

            // P (Proportionnel) : Réaction à l'erreur actuelle.
            float P = Kp * error;

            // I (Intégral) : Réaction à l'erreur accumulée (aide à vaincre la friction).
            _integralSum += error * deltaTime;
            float I = Ki * _integralSum;

            // D (Dérivé) : Amortissement basé sur la vitesse de changement de l'erreur.
            float D = 0;
            if (!_isFirstRun)
            {
                float derivative = (error - _lastError) / deltaTime;
                D = Kd * derivative;
            }
            else
            {
                // Empêche un pic de dérivée lors de la première exécution.
                _isFirstRun = false;
            }

            _lastError = error;

            return P + I + D;
        }

        public void Reset()
        {
            _integralSum = 0;
            _lastError = 0;
            _isFirstRun = true;
        }
    }
}