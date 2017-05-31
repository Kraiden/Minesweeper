package nz.geek.nathan.minesweeper.splash

import dagger.Module
import dagger.Provides
import dagger.Subcomponent
import nz.geek.nathan.minesweeper.app.ActivityScoped

/**
 * Created by nate on 17/05/17.
 */
@ActivityScoped
@Subcomponent(modules = arrayOf(SplashModule::class))
interface SplashComponent{
    fun inject(view: SplashContract.View)
}

@Module
class SplashModule(private val view: SplashContract.View){
    @Provides
    @ActivityScoped
    fun provideView() = view
}